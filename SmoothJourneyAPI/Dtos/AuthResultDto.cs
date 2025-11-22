namespace SmoothJourneyAPI.Dtos
{
    public class AuthResultDto
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
