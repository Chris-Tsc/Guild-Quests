using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Contracts.GuildQuests
{
    public record CompleteGuildQuestRequest(
        int GuildQuestId,
        int GuildQuestOptionId
    );
}
