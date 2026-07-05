using DAL.Models;

namespace DAL.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Task<bool> CreatedForUserAsync(string appUserId);
        Task<bool> InGameNameExistsAsync(string inGameName);
        Task<Player?> GetByAppUserIdAsync(string appUserId);
        void Add(Player player);
    }
}
