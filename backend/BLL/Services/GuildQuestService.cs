using BLL.Services.Interfaces;
using DAL.Data;
using BLL.Contracts.GuildQuests;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class GuildQuestService : IGuildQuestService
    {
        private readonly AppDbContext _dbc;
        private readonly IPlayerService _playerService;

        public GuildQuestService(AppDbContext dbc, IPlayerService playerService)
        {
            _dbc = dbc;
            _playerService = playerService;
        }

        public async Task<AcceptGuildQuestResultDto> AcceptGuildQuestAsync(string appUserId, int guildQuestId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            if (player.LastEnergyResetDate.Date < todayTimeUtc)
            {
                player.Energy = 100;
                player.LastEnergyResetDate = todayTimeUtc;
            }

            var boardQuest = await _dbc.PlayerRolledGuildQuests
                .Include(x => x.GuildQuest)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == player.Id &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == todayTimeUtc);

            if (boardQuest == null || boardQuest.GuildQuest == null)
                throw new Exception("This guild quest is not available on your board today.");

            var alreadyAccepted = await _dbc.PlayerGuildQuests
                .AnyAsync(x =>
                    x.PlayerId == player.Id &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == todayTimeUtc);

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

            _dbc.PlayerGuildQuests.Add(accepted);
            await _dbc.SaveChangesAsync();

            return new AcceptGuildQuestResultDto(
                boardQuest.GuildQuest.Id,
                boardQuest.GuildQuest.Name,
                boardQuest.GuildQuest.GuildQuestType?.ToString(),
                energyCost,
                player.Energy
            );
        }

        public async Task<List<ActiveGuildQuestDto>> GetActiveGuildQuestsAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var accepted = await _dbc.PlayerGuildQuests
                .Where(x =>
                    x.PlayerId == player.Id &&
                    x.DaytimeInfoUtc == todayTimeUtc &&
                    !x.IsCompleted)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Options)
                .ToListAsync();

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
                    x.GuildQuest.Events?.BaseXP ?? 0,
                    x.IsCompleted,
                    x.GuildQuest.Options
                        .Select(o => new GuildQuestOptionDto(o.Id, o.Text))
                        .ToList()
                ))
                .ToList();
        }

        public async Task<CompleteGuildQuestResultDto> CompleteGuildQuestAsync(
            string appUserId,
            int guildQuestId,
            int guildQuestOptionId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var acceptedQuest = await _dbc.PlayerGuildQuests
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == player.Id &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == todayTimeUtc);

            if (acceptedQuest == null)
                throw new Exception("This guild quest is not accepted today.");

            if (acceptedQuest.IsCompleted)
                throw new Exception("This guild quest is already completed today.");

            if (acceptedQuest.GuildQuest == null)
                throw new Exception("Guild quest data missing.");

            var option = await _dbc.GuildQuestOptions
                .FirstOrDefaultAsync(o => o.Id == guildQuestOptionId && o.GuildQuestId == guildQuestId);

            if (option == null)
                throw new Exception("Invalid option for this guild quest.");

            int chance =
                option.BaseSuccessChance +
                (player.Strength * option.StrengthWeight) +
                (player.Intelligence * option.IntelligenceWeight) +
                (player.Agility * option.AgilityWeight) +
                (player.Perception * option.PerceptionWeight) +
                player.Luck;

            chance = Math.Clamp(chance, 5, 95);

            int roll = Random.Shared.Next(0, 100);
            bool success = roll < chance;

            int fullXp = acceptedQuest.GuildQuest.Events?.BaseXP ?? 0;
            int gainedXp = success ? fullXp : (int)Math.Floor(fullXp * 0.25);

            _playerService.AddXpAndHandleLevelUps(player, gainedXp);
            acceptedQuest.IsCompleted = true;

            await _dbc.SaveChangesAsync();

            return new CompleteGuildQuestResultDto(
                success,
                gainedXp,
                player.CurrentXP,
                player.Level,
                _playerService.GetXpRequiredForNextLevel(player.Level)
            );
        }

        public async Task<List<GuildQuestBoardTestDto>> GetTodayGuildBoardAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var board = await _dbc.PlayerRolledGuildQuests
                .Where(x => x.PlayerId == player.Id && x.DaytimeInfoUtc == todayTimeUtc)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .ToListAsync();

            if (board.Count >= 10)
                return MapBoard(board);

            if (board.Count > 0)
                _dbc.PlayerRolledGuildQuests.RemoveRange(board);

            var eligible = await _dbc.GuildQuests
                .Where(q => q.RequiredLevel <= player.Level)
                .ToListAsync();

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

            _dbc.PlayerRolledGuildQuests.AddRange(rolled);
            await _dbc.SaveChangesAsync();

            board = await _dbc.PlayerRolledGuildQuests
                .Where(x => x.PlayerId == player.Id && x.DaytimeInfoUtc == todayTimeUtc)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .ToListAsync();



            return MapBoard(board);
        }

        private static List<GuildQuestBoardTestDto> MapBoard(List<PlayerRolledGuildQuest> board)
        {
            return board
                .Where(x => x.GuildQuest != null)
                .Select(x => new GuildQuestBoardTestDto(
                    x.GuildQuest!.Id,
                    x.GuildQuest.Name,
                    x.GuildQuest.Description,
                    x.GuildQuest.GuildQuestType?.ToString(),
                    x.GuildQuest.RequiredLevel,
                    x.GuildQuest.EnergyCost,
                    x.GuildQuest.EventsId,
                    x.GuildQuest.Events?.BaseXP ?? 0
                ))
                .ToList();
        }
    }
}

