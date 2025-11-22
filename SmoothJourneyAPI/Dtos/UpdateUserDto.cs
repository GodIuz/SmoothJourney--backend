namespace SmoothJourneyAPI.Dtos
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
