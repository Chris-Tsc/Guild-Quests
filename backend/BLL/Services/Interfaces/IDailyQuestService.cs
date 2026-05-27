using BLL.Contracts.DailyQuests;

namespace BLL.Services.Interfaces
{
    public interface IDailyQuestService
    {
        Task<List<DailyQuestTodayDto>> GetOrCreateTodayDailyQuestsAsync(string appUserId);

        Task<CompleteDailyQuestResultDto> CompleteDailyQuestAsync(string appUserId, int dailyQuestId, int dailyQuestOptionId);
    }
}
