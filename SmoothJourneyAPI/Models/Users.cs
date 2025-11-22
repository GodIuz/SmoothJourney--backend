using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJourneyAPI.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }

        [Required, MaxLength(50)]
        public string UserName { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [Required]
        public string PasswordSalt { get; set; } = "";

        [MaxLength(50)] public string FirstName { get; set; } = "";
        [MaxLength(50)] public string LastName { get; set; } = "";
        [MaxLength(50)] public string Country { get; set; } = "";

        public bool EmailConfirmed { get; set; } = false;
        public string? Role { get; set; } = "User";

        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshTokens>? RefreshTokens { get; set; }
    }
}
