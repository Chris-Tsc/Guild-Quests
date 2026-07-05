using BLL.Services.Interfaces;
using BLL.Contracts.Players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuildQuestsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _players;

        public PlayerController(IPlayerService players)
        {
            _players = players;
        }

        public record CreatePlayerRequest(string InGameName);

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePlayerRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var player = await _players.CreatePlayerAsync(appUserId, request.InGameName);
                

                return Ok(new
                {
                    player.Id,
                    player.InGameName,
                    player.Level,
                    player.CurrentXP,
                    player.UnspentStatPoints,
                    XpRequiredForNextLevel = _players.GetXpRequiredForNextLevel(player.Level),
                    player.Energy,
                    player.Strength,
                    player.Intelligence,
                    player.Agility,
                    player.Perception,
                    player.Luck
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { error = ex.Message });

                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("authotest")]
        public async Task<IActionResult> AuthoTest()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(appUserId))
                return Unauthorized(new { error = "Missing user id in token." });

            var player = await _players.GetMyPlayerAsync(appUserId);

            if (player == null)
                return NotFound(new { error = "Player not created yet." });

            await _players.ResetEnergyIfNeededAsync(player);

            return Ok(new
            {
                player.Id,
                player.InGameName,
                player.Level,
                player.CurrentXP,
                player.UnspentStatPoints,
                XpRequiredForNextLevel = _players.GetXpRequiredForNextLevel(player.Level),
                player.Energy,
                player.Strength,
                player.Intelligence,
                player.Agility,
                player.Perception,
                player.Luck
            });
        }

        [Authorize]
        [HttpPost("spend-stat-points")]
        public async Task<IActionResult> SpendStatPoints(SpendStatPointRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var player = await _players.SpendStatPointsAsync(
                    appUserId,
                    request.Stat,
                    request.Points);

                return Ok(new PlayerStatsDto(
                    player.Id,
                    player.InGameName,
                    player.Level,
                    player.CurrentXP,                 
                    _players.GetXpRequiredForNextLevel(player.Level),
                    player.UnspentStatPoints,
                    player.Energy,
                    player.Strength,
                    player.Intelligence,
                    player.Agility,
                    player.Perception,
                    player.Luck
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
