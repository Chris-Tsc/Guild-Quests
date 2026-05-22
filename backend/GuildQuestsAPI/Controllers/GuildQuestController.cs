using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BLL.Contracts.GuildQuests;

namespace GuildQuestsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuildQuestController : ControllerBase
    {
        private readonly IGuildQuestService _guildQuests;

        public GuildQuestController(IGuildQuestService guildQuests)
        {
            _guildQuests = guildQuests;
        }

        [Authorize]
        [HttpGet("board")]
        public async Task<IActionResult> Board()
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var board = await _guildQuests.GetTodayGuildBoardAsync(appUserId);

                return Ok(board);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("accept")]
        public async Task<IActionResult> Accept(AcceptGuildQuestRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var result = await _guildQuests.AcceptGuildQuestAsync(appUserId, request.GuildQuestId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already accepted", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { error = ex.Message });

                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("active")]
        public async Task<IActionResult> Accepted()
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var quests = await _guildQuests.GetActiveGuildQuestsAsync(appUserId);

                return Ok(quests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("complete")]
        public async Task<IActionResult> Complete(CompleteGuildQuestRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var result = await _guildQuests.CompleteGuildQuestAsync(
                    appUserId,
                    request.GuildQuestId,
                    request.GuildQuestOptionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already completed", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { error = ex.Message });

                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
