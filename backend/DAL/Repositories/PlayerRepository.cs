using DAL.Data;
using DAL.Models;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AppDbContext _dbc;

        public PlayerRepository(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        public async Task<bool> CreatedForUserAsync(string appUserId)
        {
            return await _dbc.Players.AnyAsync(p => p.AppUserId == appUserId);
        }

        public async Task<bool> InGameNameExistsAsync(string inGameName)
        {
            return await _dbc.Players.AnyAsync(p => p.InGameName == inGameName);
        }

        public async Task<Player?> GetByAppUserIdAsync(string appUserId)
        {
            return await _dbc.Players.FirstOrDefaultAsync(p => p.AppUserId == appUserId);
        }

        public void Add(Player player)
        {
            _dbc.Players.Add(player);
        }
    }
}
