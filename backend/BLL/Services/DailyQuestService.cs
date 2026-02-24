using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly AppDbContext _dbc;
        private readonly IPlayerService _playerService;

        public DailyQuestService(AppDbContext dbc, IPlayerService playerService)
        {
            _dbc = dbc;
            _playerService = playerService;
        }

        public async Task<List<PlayerDailyQuest>> GetOrCreateTodayDailyQuestAssignmentsAsync(string appUserId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var assigned = await _dbc.PlayerDailyQuests
                .Where(x => x.PlayerId == player.Id && x.DaytimeInfoUtc == todayTimeUtc)
                .Include(x => x.DailyQuest)
                .ToListAsync();

            if (assigned.Count >= 2)
            {
                return assigned;
            }

            var eligible = await _dbc.DailyQuests
                .Where(q => q.RequiredLevel <= player.Level)
                .ToListAsync();

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
                _dbc.PlayerDailyQuests.RemoveRange(assigned);
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

            _dbc.PlayerDailyQuests.AddRange(rows);
            await _dbc.SaveChangesAsync();

            return await _dbc.PlayerDailyQuests
                .Where(x => x.PlayerId == player.Id && x.DaytimeInfoUtc == todayTimeUtc)
                .Include(x => x.DailyQuest)
                .ToListAsync();
        }

        public async Task<List<DailyQuest>> GetOrCreateTodayDailyQuestsAsync(string appUserId)
        {
            var assigned = await GetOrCreateTodayDailyQuestAssignmentsAsync(appUserId);

            return assigned
                .Where(x => x.DailyQuest != null)
                .Select(x => x.DailyQuest!)
                .ToList();
        }

        public async Task CompleteDailyQuestAsync(string appUserId, int dailyQuestId)
        {
            var player = await _playerService.GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            var todayTimeUtc = DateTime.UtcNow.Date;

            var completeCheck = await _dbc.PlayerDailyQuests
                .Include(x => x.DailyQuest)
                .FirstOrDefaultAsync(x =>
                    x.PlayerId == player.Id &&
                    x.DailyQuestId == dailyQuestId &&
                    x.DaytimeInfoUtc == todayTimeUtc);

            if (completeCheck == null)
                throw new Exception("This daily quest is not assigned to you today.");

            if (completeCheck.IsCompleted)
                throw new Exception("This daily quest is already completed today.");

            if (completeCheck.DailyQuest == null)
                throw new Exception("Daily quest data missing.");

            // completing gives xp 
            player.CurrentXP += completeCheck.DailyQuest.BaseXP;

            completeCheck.IsCompleted = true;

            await _dbc.SaveChangesAsync();
        }
    }
}
