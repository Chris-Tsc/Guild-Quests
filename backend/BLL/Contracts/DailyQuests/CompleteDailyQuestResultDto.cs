
namespace BLL.Contracts.DailyQuests
{
    public record CompleteDailyQuestResultDto(
        bool Success,
        int GainedXP,
        int NewCurrentXP
    );
}
