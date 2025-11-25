using SmoothJourneyAPI.Dtos;
using SmoothJourneyAPI.Models;

namespace SmoothJourneyAPI.Interfaces
{
    public interface IAuthService
    {
        Task<(AuthResultDto result, Users user)> RegisterAsync(RegisterDto dto, string ip, string baseUrl);
        Task<AuthResultDto> LoginAsync(LoginDto dto, string ip);
        Task<AuthResultDto> RefreshTokenAsync(string token, string ip);
        Task RevokeRefreshTokenAsync(string token);
        Task SendEmailVerificationAsync(Users user, string baseUrl);
        Task RequestPasswordResetAsync(string email, string baseUrl);
        Task ResetPasswordAsync(string token, string newPassword);
        Task VerifyEmailTokenAsync(string token, string email);
    }
}
