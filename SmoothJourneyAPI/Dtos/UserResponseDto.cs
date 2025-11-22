namespace SmoothJourneyAPI.Dtos
{
    public class UserResponseDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Country { get; set; } = "";
        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
