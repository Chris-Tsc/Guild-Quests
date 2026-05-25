using DAL.Models;

namespace BLL.Services.Interfaces
{
    public interface IPlayerService
    {
        const int MaxLevel = 50;
        const int StatPointsPerLevel = 2;

        Task<Player> CreatePlayerAsync(string appUserId, string inGameName);
        Task<Player?> GetMyPlayerAsync(string appUserId);
        Task<Player> SpendStatPointsAsync(string appUserId, string stat, int points);
        void AddXpAndHandleLevelUps(Player player, int gainedXp);
        int GetXpRequiredForNextLevel(int level);
    }
}
