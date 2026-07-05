using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class DailyQuestRepository : IDailyQuestRepository
    {
        private readonly AppDbContext _dbc;

        public DailyQuestRepository(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        //Gets a player's daily quest assignments for today
        public async Task<List<PlayerDailyQuest>> GetTodayPlayerDailyQuestsWithOptionsAsync(int playerId, DateTime dayUtc)
        {
            return await _dbc.PlayerDailyQuests
                .Where(x => x.PlayerId == playerId && x.DaytimeInfoUtc == dayUtc)
                .Include(x => x.DailyQuest)
                    .ThenInclude(q => q!.Options)
                .Include(x => x.DailyQuest)
                    .ThenInclude(q => q!.Events)
                .ToListAsync();
        }

        //Gets all quests that a player is able to recieve based on their level
        public async Task<List<DailyQuest>> GetEligibleDailyQuestsAsync(int playerLevel)
        {
            return await _dbc.DailyQuests
                .Where(q => q.RequiredLevel <= playerLevel)
                .ToListAsync();
        }

        //Checks whether a player has been assigned the quest they are about to complete
        public async Task<PlayerDailyQuest?> GetPlayerDailyQuestCheckForCompletionAsync(int playerId, int dailyQuestId, DateTime dayUtc)
        {
            return await _dbc.PlayerDailyQuests
                .Include(x => x.DailyQuest)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == playerId &&
                    x.DailyQuestId == dailyQuestId &&
                    x.DaytimeInfoUtc == dayUtc);
        }

        //Checks whether the quest options truly belong to the selected daily quest
        public async Task<DailyQuestOption?> GetOptionForQuestAsync(int dailyQuestId, int optionId)
        {
            return await _dbc.DailyQuestOptions
                .FirstOrDefaultAsync(o => o.Id == optionId && o.DailyQuestId == dailyQuestId);
        }

        //Removes any uncompleted daily quests before giving new ones
        public void RemoveLeftoverAssignments(List<PlayerDailyQuest> assignments)
        {
            _dbc.PlayerDailyQuests.RemoveRange(assignments);
        }

        public void AddTodayAssignments(List<PlayerDailyQuest> assignments)
        {
            _dbc.PlayerDailyQuests.AddRange(assignments);
        }
    }
}
