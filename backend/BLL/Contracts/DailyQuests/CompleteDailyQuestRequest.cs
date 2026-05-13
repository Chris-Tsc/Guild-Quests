
namespace BLL.Contracts.DailyQuests
{
    public record CompleteDailyQuestRequest(
        int DailyQuestId,
        int DailyQuestOptionId
    );
}
