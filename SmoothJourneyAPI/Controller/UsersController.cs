using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmoothJourneyAPI.Data;
using SmoothJourneyAPI.Dtos;
using SmoothJourneyAPI.Interfaces;
using SmoothJourneyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmoothJourneyAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserRepository _users;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _cfg;

        public UsersController(IAuthService auth, IUserRepository users, ILogger<UsersController> logger, IConfiguration cfg)
        {
            _auth = auth;
            _users = users;
            _logger = logger;
            _cfg = cfg;
        }

        // ---------------- AUTH ----------------

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var baseUrl = _cfg["FrontendBaseUrl"];
            var (result, user) = await _auth.RegisterAsync(dto, ip, baseUrl);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _auth.LoginAsync(dto, ip);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _auth.RefreshTokenAsync(refreshToken, ip);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            await _auth.RevokeRefreshTokenAsync(refreshToken);
            return Ok(new { message = "Token revoked" });
        }

        // ---------------- ACCOUNT ----------------

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = long.Parse(User.FindFirstValue("UserId"));
            var user = await _users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.UserId,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Country,
                user.DateOfBirth,
                user.CreatedAt
            });
        }

        // ---------------- ADMIN ----------------

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 50)
        {
            var users = await _users.GetAllAsync(page, pageSize);
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UpdateUserDto dto)
        {
            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound();

            if (dto.UserName != null) user.UserName = dto.UserName;
            if (dto.Email != null) user.Email = dto.Email;
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.Country != null) user.Country = dto.Country;
            if (dto.DateOfBirth != null) user.DateOfBirth = dto.DateOfBirth;

            await _users.SaveChangesAsync();
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var user = await _users.GetByIdAsync(id);
            if (user == null) return NotFound();

            await _users.DeleteAsync(user);
            await _users.SaveChangesAsync();

            return Ok(new { message = "User deleted" });
        }

        // ---------------- EMAIL / PASSWORD ----------------

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            await _auth.VerifyEmailTokenAsync(token, email);
            return Ok(new { message = "Email verified successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var baseUrl = _cfg["FrontendBaseUrl"];
            await _auth.RequestPasswordResetAsync(dto.Email, baseUrl);
            return Ok(new { message = "If the email exists, reset link was sent" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(new { message = "Password updated" });
        }
    }
}
