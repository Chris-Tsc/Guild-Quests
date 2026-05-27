using BLL.Contracts.DailyQuests;
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
                .ThenInclude(q => q!.Options)
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
                .ThenInclude(q => q!.Options)
                .ToListAsync();
        }

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
                        .ToList()
                ))
                .ToList();
        }

        public async Task<CompleteDailyQuestResultDto> CompleteDailyQuestAsync(string appUserId, int dailyQuestId, int dailyQuestOptionId)
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

            // make sure option belongs to this DailyQuest
            var option = await _dbc.DailyQuestOptions
                .FirstOrDefaultAsync(o => o.Id == dailyQuestOptionId && o.DailyQuestId == dailyQuestId);

            if (option == null)
                throw new Exception("Invalid option for this daily quest.");

            // success chance calculating formula
            int chance = CalculateSuccessChance(player, option, completeCheck.DailyQuest);


            int roll = Random.Shared.Next(0, 100);
            bool success = roll < chance;

            int fullXp = completeCheck.DailyQuest.BaseXP;
            int gainedXp = success ? fullXp : (int)Math.Floor(fullXp * 0.25);

            _playerService.AddXpAndHandleLevelUps(player, gainedXp);


            completeCheck.IsCompleted = true;

            await _dbc.SaveChangesAsync();
            return new CompleteDailyQuestResultDto(
                success,
                gainedXp,
                player.CurrentXP,
                player.Level,
                _playerService.GetXpRequiredForNextLevel(player.Level)
            );
        }

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
