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

        // GET: Create Claim Form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // GET: My Claims
        public async Task<IActionResult> Index()
        {
            var lecturerName = User.Identity?.Name ?? "DemoLecturer";
            var claims = await _storageService.GetClaimsByLecturerAsync(lecturerName);
            return View(claims);
        }

        // GET: All Claims (for coordinators)
        public async Task<IActionResult> ViewAll()
        {
            var claims = await _storageService.GetAllClaimsAsync();
            return View(claims);
        }

        // GET: Manage Approvals
        public async Task<IActionResult> ManageApprovals()
        {
            var claims = await _storageService.GetAllClaimsAsync();
            return View(claims);
        }

        // POST: Create Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "⚠️ Failed to submit claim. Please check your input.";
                return View(claim);
            }

            try
            {
                string fileUrl = string.Empty;
                if (claim.DocumentFile != null && claim.DocumentFile.Length > 0)
                {
                    fileUrl = await _storageService.UploadFileAsync(claim.DocumentFile);
                }

                claim.LecturerName = User.Identity?.Name ?? "DemoLecturer";
                claim.Status = "Pending";
                claim.DocumentUrl = fileUrl;

                await _storageService.AddClaimAsync(claim);

                TempData["SuccessMessage"] = "✅ Claim submitted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error submitting claim: {ex.Message}";
                return View(claim);
            }
        }

        // POST: Approve Claim
        [HttpPost]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "⚠️ Invalid claim ID.";
                return RedirectToAction("ManageApprovals");
            }

            await _storageService.UpdateClaimStatusAsync(id, "Approved");
            TempData["SuccessMessage"] = "✅ Claim approved successfully!";
            return RedirectToAction("ManageApprovals");
        }

        // POST: Reject Claim
        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "⚠️ Invalid claim ID.";
                return RedirectToAction("ManageApprovals");
            }

            await _storageService.UpdateClaimStatusAsync(id, "Rejected");
            TempData["SuccessMessage"] = "🚫 Claim rejected successfully!";
            return RedirectToAction("ManageApprovals");
        }
    }
}
