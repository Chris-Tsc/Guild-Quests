using BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace GuildQuestsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppController : ControllerBase
    {
        private readonly AuthenticationService _auth;

        public AppController(AuthenticationService auth)
        {
            _auth = auth;
        }

        public record RegisterRequest(string Username, string Password);
        public record LoginRequest(string Username, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                var token = await _auth.RegisterAsync(request.Username, request.Password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { error = ex.Message });

                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var token = await _auth.LoginAsync(request.Username, request.Password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid credentials", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { error = ex.Message });

                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
