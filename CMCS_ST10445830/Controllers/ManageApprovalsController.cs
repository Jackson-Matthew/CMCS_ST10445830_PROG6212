using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;

namespace CMCS_ST10445830.Controllers
{
    public class ManageApprovalsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<ManageApprovalsController> _logger;

        public ManageApprovalsController(AuthDbContext context, ILogger<ManageApprovalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Shows only pending claims
        public async Task<IActionResult> Index()
        {
            try
            {
                var pendingClaims = await _context.Claims
                    .Where(c => c.Status == "Pending")
                    .ToListAsync();

                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending claims");
                TempData["ErrorMessage"] = "Error loading claims for approval.";
                return View(new List<Claim>());
            }
        }

        // Shows all claims (approved, rejected, pending)
        public async Task<IActionResult> AllClaims()
        {
            try
            {
                var allClaims = await _context.Claims.ToListAsync();
                return View(allClaims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all claims");
                TempData["ErrorMessage"] = "Error loading claims.";
                return View(new List<Claim>());
            }
        }

        // Approve a claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Claim not found.";
                    return RedirectToAction("Index");
                }

                claim.Status = "Approved";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Claim approved successfully!";
                _logger.LogInformation($"Claim {id} approved");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error approving claim: {ex.Message}";
                _logger.LogError(ex, $"Error approving claim {id}");
                return RedirectToAction("Index");
            }
        }

        // Reject a claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "⚠️ Claim not found.";
                    return RedirectToAction("Index");
                }

                claim.Status = "Rejected";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "🚫 Claim rejected successfully!";
                _logger.LogInformation($"Claim {id} rejected");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Error rejecting claim: {ex.Message}";
                _logger.LogError(ex, $"Error rejecting claim {id}");
                return RedirectToAction("Index");
            }
        }
    }
}
