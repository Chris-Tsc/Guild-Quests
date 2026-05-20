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

