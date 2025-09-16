using System.ComponentModel.DataAnnotations;

namespace CMCS_ST10445830.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required]
        public string LecturerName { get; set; }

        [Required]
        public int HoursWorked { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        public string Notes { get; set; }

        public string Status { get; set; }
    }
}
