using DAL.Models;

namespace BLL.Services.Interfaces
{
    public interface IDailyQuestService
    {
        Task<List<DailyQuest>> GetOrCreateTodayDailyQuestsAsync(string appUserId);

        Task CompleteDailyQuestAsync(string appUserId, int dailyQuestId);
    }
}
