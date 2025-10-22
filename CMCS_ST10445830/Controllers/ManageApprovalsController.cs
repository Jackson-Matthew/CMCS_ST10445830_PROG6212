using Microsoft.AspNetCore.Mvc;
using CMCS_ST10445830.Models;
using CMCS_ST10445830.Services;

namespace CMCS_ST10445830.Controllers
{
    public class ManageApprovalsController : Controller
    {
        private readonly IInMemoryStorageService _storageService;
        private readonly ILogger<ManageApprovalsController> _logger;

        public ManageApprovalsController(IInMemoryStorageService storageService, ILogger<ManageApprovalsController> logger)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var claims = await _storageService.GetAllClaimsAsync();
                var pendingClaims = claims.Where(c => c.Status == "Pending").ToList();
                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending claims");
                TempData["ErrorMessage"] = "Error loading claims for approval.";
                return View(new List<Claim>());
            }
        }

        public async Task<IActionResult> AllClaims()
        {
            try
            {
                var claims = await _storageService.GetAllClaimsAsync();
                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all claims");
                TempData["ErrorMessage"] = "Error loading claims.";
                return View(new List<Claim>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "⚠️ Invalid claim ID.";
                return RedirectToAction("Index");
            }

            try
            {
                await _storageService.UpdateClaimStatusAsync(id, "Approved");
                TempData["SuccessMessage"] = "✅ Claim approved successfully!";
                _logger.LogInformation($"Claim {id} approved by coordinator");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error approving claim: {ex.Message}";
                _logger.LogError(ex, $"Error approving claim {id}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "⚠️ Invalid claim ID.";
                return RedirectToAction("Index");
            }

            try
            {
                await _storageService.UpdateClaimStatusAsync(id, "Rejected");
                TempData["SuccessMessage"] = "🚫 Claim rejected successfully!";
                _logger.LogInformation($"Claim {id} rejected by coordinator");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error rejecting claim: {ex.Message}";
                _logger.LogError(ex, $"Error rejecting claim {id}");
            }

            return RedirectToAction("Index");
        }
    }
}