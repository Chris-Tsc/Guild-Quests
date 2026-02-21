using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class PlayerService
    {
        private readonly AppDbContext _dbc;

        public PlayerService(AppDbContext dbc)
        {
            _dbc = dbc;
        }

        public async Task<Player> CreatePlayerAsync(string appUserId, string inGameName)
        {
            inGameName = (inGameName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(inGameName))
                throw new Exception("InGameName is required.");

            // One player per user
            bool alreadyHasPlayer = await _dbc.Players.AnyAsync(p => p.AppUserId == appUserId);
            if (alreadyHasPlayer)
                throw new Exception("Player already exists for this user.");

            // InGameName must be unique
            bool nameTaken = await _dbc.Players.AnyAsync(p => p.InGameName == inGameName);
            if (nameTaken)
                throw new Exception("InGameName already exists.");

            var player = new Player
            {
                AppUserId = appUserId,
                InGameName = inGameName,

                Level = 1,
                CurrentXP = 0,

                Energy = 100,
                LastEnergyResetDate = DateTime.UtcNow.Date,

                Strength = 5,
                Intelligence = 5,
                Agility = 5,
                Perception = 5,
                Luck = 5
            };

            _dbc.Players.Add(player);
            await _dbc.SaveChangesAsync();

            return player;
        }

        public async Task<Player?> GetMyPlayerAsync(string appUserId)
        {
            return await _dbc.Players.FirstOrDefaultAsync(p => p.AppUserId == appUserId);
        }
    }
}
