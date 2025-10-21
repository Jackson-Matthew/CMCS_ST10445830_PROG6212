using CMCS_ST10445830.Models;

namespace CMCS_ST10445830.Services
{
    public class InMemoryStorageService : IInMemoryStorageService
    {
        private static readonly List<Claim> _claims = new List<Claim>();
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<InMemoryStorageService> _logger;

        public InMemoryStorageService(IWebHostEnvironment environment, ILogger<InMemoryStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public Task AddClaimAsync(Claim claim)
        {
            claim.Id = Guid.NewGuid().ToString();
            claim.RowKey = claim.Id;
            claim.PartitionKey = "claims";
            claim.CreatedDate = DateTime.UtcNow;

            _claims.Add(claim);
            _logger.LogInformation($"Claim added: {claim.Id} for lecturer {claim.LecturerName}");

            return Task.CompletedTask;
        }

        public Task<List<Claim>> GetClaimsByLecturerAsync(string lecturerName)
        {
            var claims = _claims.Where(c => c.LecturerName == lecturerName).ToList();
            _logger.LogInformation($"Retrieved {claims.Count} claims for lecturer {lecturerName}");

            return Task.FromResult(claims);
        }

        public Task<List<Claim>> GetAllClaimsAsync()
        {
            _logger.LogInformation($"Retrieved all {_claims.Count} claims");
            return Task.FromResult(_claims.ToList());
        }

        public Task UpdateClaimStatusAsync(string claimId, string status)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                claim.Status = status;
                _logger.LogInformation($"Updated claim {claimId} status to {status}");
            }

            return Task.CompletedTask;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            try
            {
                // Save file to wwwroot/uploads folder
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded: {fileName}");

                // Return relative URL
                return $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }
    }
}