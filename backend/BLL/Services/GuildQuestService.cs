using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using BLL.Contracts.GuildQuests;
using DAL.Models;

namespace BLL.Services
{
    public class GuildQuestService : IGuildQuestService
    {
        private readonly IGuildQuestRepository _guildQuests;
        private readonly IPlayerService _playerService;
        private readonly IUnitOfWork _unitOfWork;

        public GuildQuestService(IGuildQuestRepository guildQuests, IPlayerService playerService, IUnitOfWork unitOfWork)
        {
            _guildQuests = guildQuests;
            _playerService = playerService;
            _unitOfWork = unitOfWork;
        }

        //Handles the logic for when a player accepts a guild quest from the guild board
        public async Task<AcceptGuildQuestResultDto> AcceptGuildQuestAsync(string appUserId, int guildQuestId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            await _playerService.ResetEnergyIfNeededAsync(player);

            var boardQuest = await _guildQuests.GetQuestFromBoardAsync(player.Id, guildQuestId, todayTimeUtc);

            if (boardQuest == null || boardQuest.GuildQuest == null)
                throw new Exception("This guild quest is not available on your board today.");

            var alreadyAccepted = await _guildQuests.HasAcceptedQuestTodayAsync(
                player.Id,
                guildQuestId,
                todayTimeUtc);

            if (alreadyAccepted)
                throw new Exception("This guild quest is already accepted today.");

            var energyCost = boardQuest.GuildQuest.EnergyCost;

            if (player.Energy < energyCost)
                throw new Exception("Not enough energy.");

            player.Energy -= energyCost;

            var accepted = new PlayerGuildQuest
            {
                PlayerId = player.Id,
                GuildQuestId = guildQuestId,
                DaytimeInfoUtc = todayTimeUtc,
                IsCompleted = false
            };

            _guildQuests.AddAcceptedQuest(accepted);
            await _unitOfWork.SaveChangesAsync();

            return new AcceptGuildQuestResultDto(
                boardQuest.GuildQuest.Id,
                boardQuest.GuildQuest.Name,
                boardQuest.GuildQuest.GuildQuestType?.ToString(),
                energyCost,
                player.Energy
            );
        }

        //Returns accepted but incompleted guild quests that the player has for the day
        public async Task<List<ActiveGuildQuestDto>> GetActiveGuildQuestsAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            await _playerService.ResetEnergyIfNeededAsync(player);

            var accepted = await _guildQuests.GetActiveAcceptedQuestsAsync(player.Id, todayTimeUtc);

            return accepted
                .Where(x => x.GuildQuest != null)
                .Select(x => new ActiveGuildQuestDto(
                    x.GuildQuest!.Id,
                    x.GuildQuest.Name,
                    x.GuildQuest.Description,
                    x.GuildQuest.GuildQuestType?.ToString(),
                    x.GuildQuest.RequiredLevel,
                    x.GuildQuest.EnergyCost,
                    x.GuildQuest.EventsId,
                    x.GuildQuest.BaseXP,
                    x.IsCompleted,
                    x.GuildQuest.Options
                        .Select(o => new GuildQuestOptionDto(o.Id, o.Text))
                        .ToList(),
                    x.GuildQuest.Events?.EventCategory?.ToString()
                ))
                .ToList();
        }

        //Handles the logic for completing one accepted guild quest
        public async Task<CompleteGuildQuestResultDto> CompleteGuildQuestAsync(
            string appUserId,
            int guildQuestId,
            int guildQuestOptionId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            await _playerService.ResetEnergyIfNeededAsync(player);

            var acceptedQuest = await _guildQuests.GetAcceptedQuestCheckForCompletionAsync(
                player.Id,
                guildQuestId,
                todayTimeUtc);

            if (acceptedQuest == null)
                throw new Exception("This guild quest is not accepted today.");

            if (acceptedQuest.IsCompleted)
                throw new Exception("This guild quest is already completed today.");

            if (acceptedQuest.GuildQuest == null)
                throw new Exception("Guild quest data missing.");

            var option = await _guildQuests.GetOptionForQuestAsync(guildQuestId, guildQuestOptionId);

            if (option == null)
                throw new Exception("Invalid option for this guild quest.");

            // success chance calculating formula
            int chance = CalculateSuccessChance(player, option, acceptedQuest.GuildQuest);

            int roll = Random.Shared.Next(0, 100);
            bool success = roll < chance;

            int fullXp = acceptedQuest.GuildQuest.BaseXP;
            int gainedXp = success ? fullXp : (int)Math.Floor(fullXp * 0.25);

            var oldLevel = player.Level;

            _playerService.AddXpAndHandleLevelUps(player, gainedXp);

            var leveledUp = player.Level > oldLevel;
            var statPointsGained = leveledUp ? IPlayerService.StatPointsPerLevel : 0;

            acceptedQuest.IsCompleted = true;

            await _unitOfWork.SaveChangesAsync();

            return new CompleteGuildQuestResultDto(
                success,
                gainedXp,
                player.CurrentXP,
                player.Level,
                _playerService.GetXpRequiredForNextLevel(player.Level),
                leveledUp,
                statPointsGained
            );
        }

        //Returns today's rolled guild quests for the player, and keeps returning the same quests until day changes
        public async Task<List<GuildQuestBoardTestDto>> GetTodayGuildBoardAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var acceptedToday = await _guildQuests.GetAcceptedTodayAsync(player.Id, todayTimeUtc);

            var board = await _guildQuests.GetBoardTodayAsync(player.Id, todayTimeUtc);

            if (board.Count >= 10)
                return MapBoard(board, acceptedToday);

            if (board.Count > 0)
                _guildQuests.RemoveLeftoverRolledBoard(board);

            var eligible = await _guildQuests.GetEligibleGuildQuestsAsync(player.Level);

            if (eligible.Count < 10)
                throw new Exception("Not enough guild quests available for your level.");

            var rolled = eligible
                .OrderBy(_ => Random.Shared.Next())
                .Take(10)
                .Select(q => new PlayerRolledGuildQuest
                {
                    PlayerId = player.Id,
                    GuildQuestId = q.Id,
                    DaytimeInfoUtc = todayTimeUtc
                })
                .ToList();

            _guildQuests.AddRolledBoard(rolled);
            await _unitOfWork.SaveChangesAsync();

            board = await _guildQuests.GetBoardTodayAsync(player.Id, todayTimeUtc);



            return MapBoard(board, acceptedToday);
        }

        //Converts db rows into dtos for the frontend so that it shows correctly which quests are accepted
        private static List<GuildQuestBoardTestDto> MapBoard(List<PlayerRolledGuildQuest> board, List<PlayerGuildQuest> acceptedToday)
        {
            var acceptedByQuestId = acceptedToday
                .ToDictionary(x => x.GuildQuestId, x => x.IsCompleted);


            return board
            .Where(x => x.GuildQuest != null)
            .Select(x =>
            {
            var isAcceptedToday = acceptedByQuestId.ContainsKey(x.GuildQuestId);
            var isCompletedToday = acceptedByQuestId.TryGetValue(x.GuildQuestId, out var completed) && completed;

            return new GuildQuestBoardTestDto(
                x.GuildQuest!.Id,
                x.GuildQuest.Name,
                x.GuildQuest.Description,
                x.GuildQuest.GuildQuestType?.ToString(),
                x.GuildQuest.RequiredLevel,
                x.GuildQuest.EnergyCost,
                x.GuildQuest.EventsId,
                x.GuildQuest.BaseXP,
                isAcceptedToday,
                isCompletedToday,
                x.GuildQuest.Events?.EventCategory?.ToString()
                );
            })
            .ToList();
        }

        //Calculates the chance for a quest to be completed successfully
        private static int CalculateSuccessChance(Player player, GuildQuestOption option, GuildQuest quest)
        {
            var weightedStatScore =
                (player.Strength * option.StrengthWeight) +
                (player.Intelligence * option.IntelligenceWeight) +
                (player.Agility * option.AgilityWeight) +
                (player.Perception * option.PerceptionWeight);

            var statContribution = weightedStatScore * 0.55;
            var luckContribution = player.Luck * 0.25;
            var playerLevelBonus = player.Level * 0.75;
            var questLevelPenalty = quest.RequiredLevel * 0.6;

            var rawChance =
                option.BaseSuccessChance +
                statContribution +
                luckContribution +
                playerLevelBonus -
                questLevelPenalty;

            //makes sure success-failure is never guaranteed
            return Math.Clamp((int)Math.Round(rawChance), 5, 95);
        }
    }
}

