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

                var assigned = await _daily.GetOrCreateTodayDailyQuestAssignmentsAsync(appUserId);

                return Ok(assigned
                    .Where(q => q.DailyQuest != null)
                    .Select(q => new
                    {
                        q.DailyQuest!.Id,
                        q.DailyQuest!.Name,
                        q.DailyQuest!.Description,
                        q.DailyQuest!.BaseXP,
                        q.DailyQuest!.RequiredLevel,
                        q.DailyQuest!.EventsId,
                        q.IsCompleted
                    }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        public record CompleteDailyQuestRequest(int DailyQuestId);

        [Authorize]
        [HttpPost("complete")]
        public async Task<IActionResult> Complete(CompleteDailyQuestRequest request)
        {
            try
            {
                var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(appUserId))
                    return Unauthorized(new { error = "Missing user id in token." });

                await _daily.CompleteDailyQuestAsync(appUserId, request.DailyQuestId);

                return Ok(new { message = "Daily quest completed." });
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
