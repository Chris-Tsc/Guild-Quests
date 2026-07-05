namespace BLL.Contracts.Players
{
    public record SpendStatPointRequest(
        string Stat,
        int Points
    );
}
