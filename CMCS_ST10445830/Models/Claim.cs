using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_ST10445830.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }
        public User? Lecturer { get; set; }

        public int? CoordinatorId { get; set; }
        public User? Coordinator { get; set; }

        public int? ManagerId { get; set; }
        public User? Manager { get; set; }

        [Required]
        [Range(1, 9999, ErrorMessage = "Hours worked must be at least 1")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Hourly rate must be greater than 0")]
        public decimal HourlyRateAtSubmission { get; set; }

        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRateAtSubmission;

        public string? DocumentationPath { get; set; }

        [NotMapped]
        public IFormFile? DocumentFile { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}