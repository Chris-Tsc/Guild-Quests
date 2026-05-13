
namespace BLL.Contracts.DailyQuests
{
    public record DailyQuestTodayDto(
        int Id,
        string? Name,
        string? Description,
        int BaseXP,
        int RequiredLevel,
        int EventsId,
        bool IsCompleted,
        List<DailyQuestOptionDto> Options
    );
}
