using CMCS_ST10445830.Models;

namespace CMCS_ST10445830.Services
{
    public interface IInMemoryStorageService
    {
        Task AddClaimAsync(Claim claim);
        Task<List<Claim>> GetClaimsByLecturerAsync(string lecturerName);
        Task<List<Claim>> GetAllClaimsAsync();
        Task UpdateClaimStatusAsync(string claimId, string status);
        Task<string> UploadFileAsync(IFormFile file);
    }
}