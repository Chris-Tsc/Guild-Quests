namespace BLL.Contracts.GuildQuests
{
    public record ActiveGuildQuestDto(
        int Id,
        string? Name,
        string? Description,
        string? GuildQuestType,
        int RequiredLevel,
        int EnergyCost,
        int EventsId,
        int BaseXP,
        bool IsCompleted,
        List<GuildQuestOptionDto> Options,
        string? EventCategory
    );
}
