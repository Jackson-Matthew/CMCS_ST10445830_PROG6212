using System.ComponentModel.DataAnnotations;

namespace CMCS_ST10445830.Models
{
    public class Upload
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a file to upload.")]
        [Display(Name = "Upload File")]
        public IFormFile File { get; set; }

        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public long FileSize { get; set; }
    }
}