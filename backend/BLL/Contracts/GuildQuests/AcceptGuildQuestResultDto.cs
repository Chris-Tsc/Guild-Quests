using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Contracts.GuildQuests
{
    public record AcceptGuildQuestResultDto(
        int GuildQuestId,
        string? Name,
        string? GuildQuestType,
        int EnergySpent,
        int RemainingEnergy
    );
}
