using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;
using System.Security.Claims;

namespace CMCS_ST10445830.Controllers
{
    [Authorize(Roles = "Lecturer,Academic coordinator,HR")]
    public class ClaimsController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<ClaimsController> _logger;

        public ClaimsController(AuthDbContext context, ILogger<ClaimsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Claims (for Lecturers - only their claims)
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int lecturerId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var claims = await _context.Claims
                .Include(c => c.Coordinator)
                .Include(c => c.Manager)
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // GET: Claims/ViewAll (for Academic coordinators/HR - all claims)
        [Authorize(Roles = "Academic coordinator,HR")]
        public async Task<IActionResult> ViewAll()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Coordinator)
                .Include(c => c.Manager)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }

        // GET: Claims/Create
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int lecturerId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get the lecturer's profile information
            var lecturer = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == lecturerId);

            if (lecturer?.UserProfile == null || lecturer.UserProfile.HourlyRate <= 0)
            {
                TempData["ErrorMessage"] = "Your profile is incomplete. Please contact HR to set up your profile and hourly rate.";
                return RedirectToAction("Index");
            }

            // Pass all user info to the view for display
            ViewBag.UserHourlyRate = lecturer.UserProfile.HourlyRate;
            ViewBag.UserFirstName = lecturer.UserProfile.FirstName;
            ViewBag.UserLastName = lecturer.UserProfile.LastName;
            ViewBag.UserEmail = lecturer.UserProfile.Email;

            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create(CMCS_ST10445830.Models.Claim claim)
        {
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int lecturerId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get the lecturer with their profile
            var lecturer = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == lecturerId);

            if (lecturer?.UserProfile == null || lecturer.UserProfile.HourlyRate <= 0)
            {
                TempData["ErrorMessage"] = "Your profile is incomplete. Please contact HR to set up your hourly rate.";
                return View(claim);
            }

            // VALIDATION 1: Check if hours exceed monthly limit (180 hours)
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked cannot exceed 180 hours per month.");
            }

            // VALIDATION 2: Check if hours are at least 1
            if (claim.HoursWorked < 1)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked must be at least 1 hour.");
            }

            // VALIDATION 3: Check for reasonable maximum (optional but good practice)
            if (claim.HoursWorked > 300)
            {
                ModelState.AddModelError("HoursWorked", "Hours worked seem unusually high. Please verify the amount.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set the LecturerId from the logged-in user
                    claim.LecturerId = lecturerId;

                    // AUTO-PULL: Set the HourlyRateAtSubmission from the user's profile
                    claim.HourlyRateAtSubmission = lecturer.UserProfile.HourlyRate;

                    claim.Status = "Pending";
                    claim.SubmittedAt = DateTime.UtcNow;

                    // Handle file upload
                    if (claim.DocumentFile != null && claim.DocumentFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + claim.DocumentFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await claim.DocumentFile.CopyToAsync(stream);
                        }

                        claim.DocumentationPath = uniqueFileName;
                    }

                    _context.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Claim submitted successfully! Total amount: {claim.TotalAmount:C}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating claim for user {UserId}", lecturerId);
                    ModelState.AddModelError("", "An error occurred while submitting your claim. Please try again.");
                }
            }

            // If we get here, there were validation errors - reload user data for the view
            ViewBag.UserHourlyRate = lecturer.UserProfile.HourlyRate;
            ViewBag.UserFirstName = lecturer.UserProfile.FirstName;
            ViewBag.UserLastName = lecturer.UserProfile.LastName;
            ViewBag.UserEmail = lecturer.UserProfile.Email;

            return View(claim);
        }

        // GET: Claims/Approve/5 (for Academic coordinators)
        [Authorize(Roles = "Academic coordinator")]
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Claims/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Academic coordinator")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.UserProfile)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(Manage));
                }

                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int coordinatorId))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                claim.CoordinatorId = coordinatorId;
                claim.Status = "Approved";
                claim.ReviewedAt = DateTime.UtcNow;

                _context.Update(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Claim from {claim.Lecturer?.UserProfile?.FirstName} {claim.Lecturer?.UserProfile?.LastName} approved successfully! Amount: {claim.TotalAmount:C}";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving claim {ClaimId}", id);
                TempData["ErrorMessage"] = "An error occurred while approving the claim.";
                return RedirectToAction(nameof(Manage));
            }
        }

        // GET: Claims/Reject/5 (for Academic coordinators)
        [Authorize(Roles = "Academic coordinator")]
        public async Task<IActionResult> Reject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Claims/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Academic coordinator")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var claim = await _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.UserProfile)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction(nameof(Manage));
                }

                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int coordinatorId))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                claim.CoordinatorId = coordinatorId;
                claim.Status = "Rejected";
                claim.ReviewedAt = DateTime.UtcNow;

                _context.Update(claim);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Claim from {claim.Lecturer?.UserProfile?.FirstName} {claim.Lecturer?.UserProfile?.LastName} has been rejected.";
                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting claim {ClaimId}", id);
                TempData["ErrorMessage"] = "An error occurred while rejecting the claim.";
                return RedirectToAction(nameof(Manage));
            }
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Coordinator)
                .Include(c => c.Manager)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound();
            }

            // Security check: Users can only see their own claims unless they're academic coordinators/HR
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int currentUserId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var isCoordinatorOrHR = User.IsInRole("Academic coordinator") || User.IsInRole("HR");
            if (!isCoordinatorOrHR && claim.LecturerId != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View(claim);
        }

        // GET: Claims/Manage (for Academic coordinators - pending claims)
        [Authorize(Roles = "Academic coordinator")]
        public async Task<IActionResult> Manage()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .ThenInclude(l => l.UserProfile)
                .Where(c => c.Status == "Pending")
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(pendingClaims);
        }
    }
}