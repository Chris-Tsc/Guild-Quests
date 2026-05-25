using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Contracts.Players
{
    public record SpendStatPointRequest(
        string Stat,
        int Points
    );
}
