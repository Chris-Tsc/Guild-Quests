
namespace BLL.Contracts.DailyQuests
{
    public record CompleteDailyQuestResultDto(
        bool Success,
        int GainedXP,
        int NewCurrentXP,
        int NewLevel,
        int XpRequiredForNextLevel,
        bool LeveledUp,
        int StatPointsGained
    );
}
