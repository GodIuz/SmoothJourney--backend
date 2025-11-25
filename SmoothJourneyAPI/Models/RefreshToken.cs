using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmoothJourneyAPI.Models
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string Token { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public long UserId { get; set; }
        [ForeignKey(nameof(UserId))] 
        public Users? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
    }
}