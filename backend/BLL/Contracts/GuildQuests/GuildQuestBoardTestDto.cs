using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Contracts.GuildQuests
{
    public record GuildQuestBoardTestDto
    (
        int Id,
        string? Name,
        string? Description,
        string? GuildQuestType,
        int RequiredLevel,
        int EnergyCost,
        int EventsId,
        int BaseXP,
        bool IsAcceptedToday,
        bool IsCompletedToday
    );
}
