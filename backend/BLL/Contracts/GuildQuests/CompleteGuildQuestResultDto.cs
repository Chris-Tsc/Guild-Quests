using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Contracts.GuildQuests
{
    public record CompleteGuildQuestResultDto(
        bool Success,
        int GainedXP,
        int NewCurrentXP,
        int NewLevel,
        int XpRequiredForNextLevel,
        bool LeveledUp,
        int StatPointsGained
    );
}
