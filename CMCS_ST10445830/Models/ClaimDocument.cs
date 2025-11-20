namespace CMCS_ST10445830.Models
{
    public class ClaimDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // /uploads/xxxx.pdf
        public DateTime UploadedAt { get; set; }
    }

}
