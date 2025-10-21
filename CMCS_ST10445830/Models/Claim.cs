using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_ST10445830.Models
{
    public class Claim
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PartitionKey { get; set; } = "claims";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string LecturerName { get; set; } = string.Empty;

        [Required]
        public string Month { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Hours worked must be at least 1")]
        public int HoursWorked { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        public double HourlyRate { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string DocumentUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // For file uploads only
        [NotMapped]
        public IFormFile? DocumentFile { get; set; }

        [NotMapped]
        public double TotalAmount => HoursWorked * HourlyRate;
    }
}