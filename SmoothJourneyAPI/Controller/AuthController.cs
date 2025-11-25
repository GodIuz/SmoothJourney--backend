using Microsoft.AspNetCore.Mvc;
using SmoothJourneyAPI.Data;
using SmoothJourneyAPI.Interfaces;
using SmoothJourneyAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace SmoothJourneyAPI.Controller
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly SmoothJourneyDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthController(IAuthService auth, SmoothJourneyDbContext db, IConfiguration cfg) { _auth = auth; _db = db; _cfg = cfg; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var baseUrl = Request.Headers["Origin"].ToString() ?? _cfg["FrontendBaseUrl"] ?? "https://yourfrontend";
            var (res, user) = await _auth.RegisterAsync(dto, ip, baseUrl);

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpiresAt.AddDays(30)
            });

            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var res = await _auth.LoginAsync(dto, ip);

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpiresAt.AddDays(30)
            });

            return Ok(res);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token)) return Unauthorized();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var res = await _auth.RefreshTokenAsync(token, ip);

            Response.Cookies.Append("refreshToken", res.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = res.AccessTokenExpiresAt.AddDays(30)
            });

            return Ok(res);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var token)) return BadRequest();
            await _auth.RevokeRefreshTokenAsync(token);
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> Forgot([FromBody] ForgotPasswordDto dto)
        {
            var baseUrl = Request.Headers["Origin"].ToString() ?? _cfg["FrontendBaseUrl"] ?? "https://yourfrontend";
            await _auth.RequestPasswordResetAsync(dto.Email, baseUrl);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Reset([FromBody] ResetPasswordDto dto)
        {
            await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok();
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> Verify([FromQuery] string token, [FromQuery] string email)
        {
            await _auth.VerifyEmailTokenAsync(token, email);
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me([FromServices] SmoothJourneyDbContext db)
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (!long.TryParse(idClaim, out var userId)) return Unauthorized();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();
            return Ok(new { user.UserId, user.UserName, user.Email, user.FirstName, user.LastName, user.Country, user.DateOfBirth, user.EmailConfirmed });
        }
    }
}
