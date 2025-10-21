using CMCS_ST10445830.Models;
using CMCS_ST10445830.Services;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    public class UploadController : Controller
    {
        private readonly IInMemoryStorageService _storageService;

        public UploadController(IInMemoryStorageService storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
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
            if (ModelState.IsValid && model.File != null && model.File.Length > 0)
            {
                try
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(model.File.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("File", "Invalid file type.");
                        return View(model);
                    }

                    // Upload file
                    var fileUrl = await _storageService.UploadFileAsync(model.File);

                    TempData["Message"] = $"File '{model.File.FileName}' uploaded successfully!";
                    TempData["MessageType"] = "success";

                    return View(new Upload());
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"Error uploading file: {ex.Message}";
                    TempData["MessageType"] = "danger";
                }
            }
            else
            {
                ModelState.AddModelError("File", "Please select a valid file to upload.");
            }

            return View(model);
        }
    }
}