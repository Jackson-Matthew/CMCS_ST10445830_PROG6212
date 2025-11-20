using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    public class UploadController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UploadController(AuthDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new Upload());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Upload model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
                return View(model);
            }

            // Allowed file types
            var allowedExtensions = new[]
            {
                ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"
            };

            var fileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("File", "Invalid file type.");
                return View(model);
            }

            try
            {
                // Ensure upload directory exists
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Generate unique file name
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                string fullPath = Path.Combine(uploadPath, uniqueFileName);

                // Save file physically
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                // Public URL for the file
                string urlPath = $"/uploads/{uniqueFileName}";

                // OPTIONAL: Save into DB if needed (e.g. Upload table)
                var uploadedFile = new ClaimDocument
                {
                    FileName = model.File.FileName,
                    FilePath = urlPath,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Add(uploadedFile);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"File '{model.File.FileName}' uploaded successfully!";
                TempData["MessageType"] = "success";

                return View(new Upload());
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error uploading file: {ex.Message}";
                TempData["MessageType"] = "danger";
                return View(model);
            }
        }
    }
}
