using DAL.Models;

namespace BLL.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<Player> CreatePlayerAsync(string appUserId, string inGameName);
        Task<Player?> GetMyPlayerAsync(string appUserId);
        void AddXpAndHandleLevelUps(Player player, int gainedXp);
        int GetXpRequiredForNextLevel(int level);
    }
}
