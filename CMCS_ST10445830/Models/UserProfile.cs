using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_ST10445830.Models
{
    public class UserProfile
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be between 0.01 and 1000")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        // Navigation property
        public virtual User User { get; set; }
    }
}