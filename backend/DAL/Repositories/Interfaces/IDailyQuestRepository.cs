using DAL.Models;

namespace DAL.Repositories.Interfaces
{
    public interface IDailyQuestRepository
    {
        Task<List<PlayerDailyQuest>> GetTodayPlayerDailyQuestsWithOptionsAsync(int playerId, DateTime dayUtc);
        Task<List<DailyQuest>> GetEligibleDailyQuestsAsync(int playerLevel);
        Task<PlayerDailyQuest?> GetPlayerDailyQuestCheckForCompletionAsync(int playerId, int dailyQuestId, DateTime dayUtc);
        Task<DailyQuestOption?> GetOptionForQuestAsync(int dailyQuestId, int optionId);

        void RemoveLeftoverAssignments(List<PlayerDailyQuest> assignments);
        void AddTodayAssignments(List<PlayerDailyQuest> assignments);
    }
}
