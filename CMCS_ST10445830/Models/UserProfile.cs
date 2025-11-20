using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_ST10445830.Models
{
    public class UserProfile
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }   // Primary Key + FK to Users.Id

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Lecturers will have this, admins may have NULL
        [Column(TypeName = "decimal(10,2)")]
        public decimal? HourlyRate { get; set; }

        // Navigation Property
        public User? User { get; set; }
    }
}
