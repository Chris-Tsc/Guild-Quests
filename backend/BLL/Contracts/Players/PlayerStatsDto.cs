namespace BLL.Contracts.Players
{
    public record PlayerStatsDto(
        int Id,
        string? InGameName,
        int Level,
        int CurrentXP,
        int XpRequiredForNextLevel,
        int UnspentStatPoints,
        int Energy,
        int Strength,
        int Intelligence,
        int Agility,
        int Perception,
        int Luck
    );
}
