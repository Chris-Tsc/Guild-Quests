using BLL.Contracts.DailyQuests;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GuildQuestsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyQuestController : ControllerBase
    {
        private readonly IDailyQuestService _daily;

        public DailyQuestController(IDailyQuestService daily)
        {
            _daily = daily;
        }

        [Authorize]
        [HttpGet("today")]
        public async Task<IActionResult> Today()
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var quests = await _daily.GetOrCreateTodayDailyQuestsAsync(appUserId);

                return Ok(quests);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("complete")]
        public async Task<IActionResult> Complete(CompleteDailyQuestRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                var result = await _daily.CompleteDailyQuestAsync(
                    appUserId,
                    request.DailyQuestId,
                    request.DailyQuestOptionId);

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
