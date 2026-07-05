using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class GuildQuestRepository : IGuildQuestRepository
    {
        private readonly AppDbContext _dbc;

        public GuildQuestRepository(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        //Checks whether this guild quest in on the player's board for the day
        public async Task<PlayerRolledGuildQuest?> GetQuestFromBoardAsync(int playerId, int guildQuestId, DateTime dayUtc)
        {
            return await _dbc.PlayerRolledGuildQuests
                .Include(x => x.GuildQuest)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == playerId &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == dayUtc);
        }

        //Checks whether a player has already accepted this guild quest for the day
        public async Task<bool> HasAcceptedQuestTodayAsync(int playerId, int guildQuestId, DateTime dayUtc)
        {
            return await _dbc.PlayerGuildQuests
                .AnyAsync(x =>
                    x.PlayerId == playerId &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == dayUtc);
        }

        //Gets all the guild quests that have been accepted by the player for the day
        public async Task<List<PlayerGuildQuest>> GetAcceptedTodayAsync(int playerId, DateTime dayUtc)
        {
            return await _dbc.PlayerGuildQuests
                .Where(x => x.PlayerId == playerId && x.DaytimeInfoUtc == dayUtc)
                .ToListAsync();
        }

        //Gets all the accepted guild quests that are active and not completed yet
        public async Task<List<PlayerGuildQuest>> GetActiveAcceptedQuestsAsync(int playerId, DateTime dayUtc)
        {
            return await _dbc.PlayerGuildQuests
                .Where(x =>
                    x.PlayerId == playerId &&
                    x.DaytimeInfoUtc == dayUtc &&
                    !x.IsCompleted)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Options)
                .ToListAsync();
        }

        //Checks whether the player has truly accepted the guild quest that they are about to complete
        public async Task<PlayerGuildQuest?> GetAcceptedQuestCheckForCompletionAsync(int playerId, int guildQuestId, DateTime dayUtc)
        {
            return await _dbc.PlayerGuildQuests
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == playerId &&
                    x.GuildQuestId == guildQuestId &&
                    x.DaytimeInfoUtc == dayUtc);
        }

        //Checks whether thiis option truly belongs to that guild quest
        public async Task<GuildQuestOption?> GetOptionForQuestAsync(int guildQuestId, int optionId)
        {
            return await _dbc.GuildQuestOptions
                .FirstOrDefaultAsync(o => o.Id == optionId && o.GuildQuestId == guildQuestId);
        }

        //Gets the player's rolled guild quests for the day
        public async Task<List<PlayerRolledGuildQuest>> GetBoardTodayAsync(int playerId, DateTime dayUtc)
        {
            return await _dbc.PlayerRolledGuildQuests
                .Where(x => x.PlayerId == playerId && x.DaytimeInfoUtc == dayUtc)
                .Include(x => x.GuildQuest)
                    .ThenInclude(q => q!.Events)
                .ToListAsync();
        }

        //Gets all the guild quests that the player is high enough level to be able to accept
        public async Task<List<GuildQuest>> GetEligibleGuildQuestsAsync(int playerLevel)
        {
            return await _dbc.GuildQuests
                .Where(q => q.RequiredLevel <= playerLevel)
                .ToListAsync();
        }

        public void AddAcceptedQuest(PlayerGuildQuest acceptedQuest)
        {
            _dbc.PlayerGuildQuests.Add(acceptedQuest);
        }

        //Removes incomplete guild board rows before rolling a fresh board
        public void RemoveLeftoverRolledBoard(List<PlayerRolledGuildQuest> board)
        {
            _dbc.PlayerRolledGuildQuests.RemoveRange(board);
        }

        public void AddRolledBoard(List<PlayerRolledGuildQuest> board)
        {
            _dbc.PlayerRolledGuildQuests.AddRange(board);
        }
    }
}
