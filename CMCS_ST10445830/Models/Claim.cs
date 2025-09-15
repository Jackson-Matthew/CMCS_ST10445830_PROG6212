using System.ComponentModel.DataAnnotations;

namespace CMCS_ST10445830.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        [Range(1,200, ErrorMessage = "Please enter valid hours.")]
        public int HoursWorked { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Please enter a valid hourly rate.")]
        public decimal HourlyRate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
