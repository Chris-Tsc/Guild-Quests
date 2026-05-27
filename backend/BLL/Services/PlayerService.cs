using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class PlayerService : IPlayerService
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
                UnspentStatPoints = 0,

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

        public void AddXpAndHandleLevelUps(Player player, int gainedXp)
        {
            if (gainedXp <= 0)
                return;

            if (player.Level >= IPlayerService.MaxLevel)
            {
                player.CurrentXP = 0;
                return;
            }

            player.CurrentXP += gainedXp;

            while (
                player.Level < IPlayerService.MaxLevel &&
                player.CurrentXP >= GetXpRequiredForNextLevel(player.Level))
            {
                var requiredXp = GetXpRequiredForNextLevel(player.Level);

                player.CurrentXP -= requiredXp;
                player.Level++;
                player.UnspentStatPoints += IPlayerService.StatPointsPerLevel;
            }

            if (player.Level >= IPlayerService.MaxLevel)
                player.CurrentXP = 0;
        }

        public int GetXpRequiredForNextLevel(int level)
        {
            if (level >= IPlayerService.MaxLevel)
                return 0;

            return level switch
            {
                1 => 400,
                2 => 700,
                _ => 700 + ((level - 2) * 350)
            };
        }

        public async Task<Player> SpendStatPointsAsync(string appUserId, string stat, int points)
        {
            if (points <= 0)
                throw new Exception("You dont have any skill points!.");

            var player = await GetMyPlayerAsync(appUserId);
            if (player == null)
                throw new Exception("Player not created yet.");

            if (player.UnspentStatPoints < points)
                throw new Exception("Something is wrong with your skill points!");

            var normalizedStat = stat.Trim().ToLowerInvariant();

            switch (normalizedStat)
            {
                case "strength":
                    player.Strength += points;
                    break;

                case "intelligence":
                    player.Intelligence += points;
                    break;

                case "agility":
                    player.Agility += points;
                    break;

                case "perception":
                    player.Perception += points;
                    break;

                case "luck":
                    player.Luck += points;
                    break;

                default:
                    throw new Exception("Invalid stat.");
            }

            player.UnspentStatPoints -= points;

            await _dbc.SaveChangesAsync();

            return player;
        }

        public async Task ResetEnergyIfNeededAsync(Player player)
        {
            var todayTimeUtc = DateTime.UtcNow.Date;

            if (player.LastEnergyResetDate.Date >= todayTimeUtc)
                return;

            player.Energy = 100;
            player.LastEnergyResetDate = todayTimeUtc;

            await _dbc.SaveChangesAsync();
        }
    }
}
