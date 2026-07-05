using BLL.Contracts.DailyQuests;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;
using DAL.Models;

namespace BLL.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IDailyQuestRepository _dailyQuests;
        private readonly IPlayerService _playerService;
        private readonly IUnitOfWork _unitOfWork;

        public DailyQuestService(IDailyQuestRepository dailyQuests, IPlayerService playerService, IUnitOfWork unitOfWork)
        {
            _dailyQuests = dailyQuests;
            _playerService = playerService;
            _unitOfWork = unitOfWork;
        }

        //Makes sure the player is assigned 2 daily quests for today, and returns the same quests for the whole day
        public async Task<List<PlayerDailyQuest>> GetOrCreateTodayDailyQuestAssignmentsAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var assigned = await _dailyQuests.GetTodayPlayerDailyQuestsWithOptionsAsync(player.Id, todayTimeUtc);

            if (assigned.Count >= 2)
            {
                return assigned;
            }

            var eligible = await _dailyQuests.GetEligibleDailyQuestsAsync(player.Level);

            if (eligible.Count < 2)
                throw new Exception("Not enough daily quests available for your level.");

            // randomly roll 2 distinct daily quests
            var rng = Random.Shared;
            var first = eligible[rng.Next(eligible.Count)];
            DailyQuest second;
            do
            {
                second = eligible[rng.Next(eligible.Count)];
            } while (second.Id == first.Id);

            if (assigned.Count > 0)
            {
                _dailyQuests.RemoveLeftoverAssignments(assigned);
            }

            var rows = new List<PlayerDailyQuest>
            {
                new PlayerDailyQuest
                {
                    PlayerId = player.Id,
                    DailyQuestId = first.Id,
                    DaytimeInfoUtc = todayTimeUtc,
                    IsCompleted = false
                },
                new PlayerDailyQuest
                {
                    PlayerId = player.Id,
                    DailyQuestId = second.Id,
                    DaytimeInfoUtc = todayTimeUtc,
                    IsCompleted = false
                }
            };

            _dailyQuests.AddTodayAssignments(rows);
            await _unitOfWork.SaveChangesAsync();

            return await _dailyQuests.GetTodayPlayerDailyQuestsWithOptionsAsync(player.Id, todayTimeUtc);
        }

        //Gets today's assignments and maps them to DTO for the frontend
        public async Task<List<DailyQuestTodayDto>> GetOrCreateTodayDailyQuestsAsync(string appUserId)
        {
            var assigned = await GetOrCreateTodayDailyQuestAssignmentsAsync(appUserId);

            return assigned
                .Where(x => x.DailyQuest != null)
                .Select(x => new DailyQuestTodayDto(
                    x.DailyQuest!.Id,
                    x.DailyQuest.Name,
                    x.DailyQuest.Description,
                    x.DailyQuest.BaseXP,
                    x.DailyQuest.RequiredLevel,
                    x.DailyQuest.EventsId,
                    x.IsCompleted,
                    x.DailyQuest.Options
                        .Select(o => new DailyQuestOptionDto(o.Id, o.Text))
                        .ToList(),
                    x.DailyQuest.Events?.EventCategory?.ToString()
                ))
                .ToList();
        }

        //Handles the logic for completing one assigned daily quest
        public async Task<CompleteDailyQuestResultDto> CompleteDailyQuestAsync(string appUserId, int dailyQuestId, int dailyQuestOptionId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var completeCheck = await _dailyQuests.GetPlayerDailyQuestCheckForCompletionAsync(
                player.Id,
                dailyQuestId,
                todayTimeUtc);

            if (completeCheck == null)
                throw new Exception("This daily quest is not assigned to you today.");

            if (completeCheck.IsCompleted)
                throw new Exception("This daily quest is already completed today.");

            if (completeCheck.DailyQuest == null)
                throw new Exception("Daily quest data missing.");

            // make sure option belongs to this DailyQuest
            var option = await _dailyQuests.GetOptionForQuestAsync(dailyQuestId, dailyQuestOptionId);

            if (option == null)
                throw new Exception("Invalid option for this daily quest.");

            // success chance calculating formula
            int chance = CalculateSuccessChance(player, option, completeCheck.DailyQuest);


            int roll = Random.Shared.Next(0, 100);
            bool success = roll < chance;

            int fullXp = completeCheck.DailyQuest.BaseXP;
            int gainedXp = success ? fullXp : (int)Math.Floor(fullXp * 0.25);

            var oldLevel = player.Level;

            _playerService.AddXpAndHandleLevelUps(player, gainedXp);

            var leveledUp = player.Level > oldLevel;
            var statPointsGained = leveledUp ? IPlayerService.StatPointsPerLevel : 0;


            completeCheck.IsCompleted = true;

            await _unitOfWork.SaveChangesAsync();
            return new CompleteDailyQuestResultDto(
                success,
                gainedXp,
                player.CurrentXP,
                player.Level,
                _playerService.GetXpRequiredForNextLevel(player.Level),
                leveledUp,
                statPointsGained
            );
        }

        //Calculates the chance for a quest to be completed successfully
        private static int CalculateSuccessChance(Player player, DailyQuestOption option, DailyQuest quest)
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
