using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmoothJourneyAPI.Data;
using SmoothJourneyAPI.Dtos;
using SmoothJourneyAPI.Interfaces;
using SmoothJourneyAPI.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmoothJourneyAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly SmoothJourneyDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly PasswordService _pwd;
        private readonly IEmailService _email;
        private readonly byte[] _key;

        public AuthService(SmoothJourneyDbContext db, IConfiguration cfg, PasswordService pwd, IEmailService email)
        {
            _db = db;
            _cfg = cfg;
            _pwd = pwd; _email = email;
            _key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]);
        }

        private string GenerateAccessToken(Users user)
        {
            var claims = new[]
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15")),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken CreateRefreshToken(string ip)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_cfg["Jwt:RefreshTokenDays"] ?? "30")),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ip
            };
        }

        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
        }

        public async Task<(AuthResultDto result, Users user)> RegisterAsync(RegisterDto dto, string ip, string baseUrl)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new ApplicationException("Email already in use");

            var (hash, salt) = _pwd.HashPassword(dto.Password);

            var user = new Users
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Country = dto.Country,
                DateOfBirth = dto.DateOfBirth,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var refresh = CreateRefreshToken(ip);
            refresh.UserId = user.UserId;
            await _db.RefreshTokens.AddAsync(refresh);

            // Email verification token (store hash)
            var verificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var ev = new EmailVerification { UserId = user.UserId, TokenHash = HashToken(verificationToken), ExpiresAt = DateTime.UtcNow.AddHours(24) };
            await _db.EmailVerifications.AddAsync(ev);

            await _db.SaveChangesAsync();

            // send verification email
            var verifyUrl = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}&email={Uri.EscapeDataString(user.Email)}";
            await _email.SendEmailAsync(user.Email, "Verify your email", $"Click <a href=\"{verifyUrl}\">here</a> to verify your email.");

            var access = GenerateAccessToken(user);
            var result = new AuthResultDto { AccessToken = access, RefreshToken = refresh.Token, AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15")) };
            return (result, user);
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto dto, string ip)
        {
            var user = await _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) throw new ApplicationException("Invalid credentials");
            if (!_pwd.Verify(user.PasswordHash, dto.Password, user.PasswordSalt)) throw new ApplicationException("Invalid credentials");

            // revoke previous tokens
            var existing = user.RefreshTokens?.Where(r => !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow).ToList();
            if (existing != null)
            {
                foreach (var t in existing) t.IsRevoked = true;
            }

            var refresh = CreateRefreshToken(ip);
            refresh.UserId = user.UserId;
            await _db.RefreshTokens.AddAsync(refresh);
            await _db.SaveChangesAsync();

            var access = GenerateAccessToken(user);
            return new AuthResultDto { AccessToken = access, RefreshToken = refresh.Token, AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15")) };
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string token, string ip)
        {
            var existing = await _db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.Token == token);
            if (existing == null || existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
                throw new ApplicationException("Invalid refresh token");

            existing.IsRevoked = true;
            var replacement = CreateRefreshToken(ip);
            replacement.UserId = existing.UserId;
            existing.ReplacedByToken = replacement.Token;

            await _db.RefreshTokens.AddAsync(replacement);
            await _db.SaveChangesAsync();

            var access = GenerateAccessToken(existing.User!);
            return new AuthResultDto { AccessToken = access, RefreshToken = replacement.Token, AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "15")) };
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
            if (existing == null) throw new ApplicationException("Invalid token");
            existing.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        public async Task SendEmailVerificationAsync(Users user, string baseUrl)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var ev = new EmailVerification { UserId = user.UserId, TokenHash = HashToken(token), ExpiresAt = DateTime.UtcNow.AddHours(24) };
            await _db.EmailVerifications.AddAsync(ev);
            await _db.SaveChangesAsync();

            var verifyUrl = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
            await _email.SendEmailAsync(user.Email, "Verify your email", $"Click <a href=\"{verifyUrl}\">here</a> to verify your email.");
        }

        public async Task RequestPasswordResetAsync(string email, string baseUrl)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return; // don't reveal
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var pr = new PasswordResetToken { UserId = user.UserId, TokenHash = HashToken(token), ExpiresAt = DateTime.UtcNow.AddHours(2) };
            await _db.PasswordResetTokens.AddAsync(pr);
            await _db.SaveChangesAsync();

            var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
            await _email.SendEmailAsync(user.Email, "Reset your password", $"Click <a href=\"{resetUrl}\">here</a> to reset your password.");
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var hash = HashToken(token);
            var pr = await _db.PasswordResetTokens.FirstOrDefaultAsync(p => p.TokenHash == hash && !p.Used && p.ExpiresAt > DateTime.UtcNow);
            if (pr == null) throw new ApplicationException("Invalid or expired token");

            var user = await _db.Users.FindAsync(pr.UserId);
            if (user == null) throw new ApplicationException("User not found");

            var (hashPwd, salt) = _pwd.HashPassword(newPassword);
            user.PasswordHash = hashPwd;
            user.PasswordSalt = salt;

            pr.Used = true;
            await _db.SaveChangesAsync();
        }

        public async Task VerifyEmailTokenAsync(string token, string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new ApplicationException("Invalid token");

            var hash = HashToken(token);
            var ev = await _db.EmailVerifications.FirstOrDefaultAsync(e => e.UserId == user.UserId && e.TokenHash == hash && !e.Used && e.ExpiresAt > DateTime.UtcNow);
            if (ev == null) throw new ApplicationException("Invalid or expired token");

            ev.Used = true;
            user.EmailConfirmed = true;
            await _db.SaveChangesAsync();
        }
    }
}
