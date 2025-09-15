using System.ComponentModel.DataAnnotations;

namespace CMCS_ST10445830.Models
{
    public class Upload
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "Upload File")]
        public IFormFile File { get; set; }
    }
}
