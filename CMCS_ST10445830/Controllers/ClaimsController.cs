using Microsoft.AspNetCore.Mvc;
using CMCS_ST10445830.Models;
using CMCS_ST10445830.Services;

namespace CMCS_ST10445830.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly IInMemoryStorageService _storageService;

        public ClaimsController(IInMemoryStorageService storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var lecturerName = User.Identity?.Name ?? "Unknown Lecturer";
            var claims = await _storageService.GetClaimsByLecturerAsync(lecturerName);
            return View(claims);
        }

        public async Task<IActionResult> ViewAll()
        {
            var claims = await _storageService.GetAllClaimsAsync();
            return View(claims);
        }

        public async Task<IActionResult> ManageApprovals()
        {
            var claims = await _storageService.GetAllClaimsAsync();
            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to submit claim. Please check your input.";
                return View(claim);
            }

            try
            {
                string fileUrl = string.Empty;
                if (claim.DocumentFile != null && claim.DocumentFile.Length > 0)
                {
                    fileUrl = await _storageService.UploadFileAsync(claim.DocumentFile);
                }
                claim.LecturerName = User.Identity?.Name ?? "Unknown Lecturer";
                claim.Status = "Pending";
                claim.DocumentUrl = fileUrl;

                await _storageService.AddClaimAsync(claim);

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error submitting claim: {ex.Message}";
                return View(claim);
            }
        }
        public IActionResult ViewDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                return BadRequest("Invalid document URL.");

            return Redirect(url);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("ManageApprovals");
            }

            try
            {
                await _storageService.UpdateClaimStatusAsync(id, "Approved");
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
            }

            return RedirectToAction("ManageApprovals");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Invalid claim ID.";
                return RedirectToAction("ManageApprovals");
            }

            try
            {
                await _storageService.UpdateClaimStatusAsync(id, "Rejected");
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            }

            return RedirectToAction("ManageApprovals");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}