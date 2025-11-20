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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Create(CMCS_ST10445830.Models.Claim claim)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue("UserId");
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int lecturerId))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Set the LecturerId from the logged-in user
                claim.LecturerId = lecturerId;
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

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(Index));
            }

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
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
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

            TempData["SuccessMessage"] = "Claim approved successfully!";
            return RedirectToAction(nameof(ViewAll));
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
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
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

            TempData["SuccessMessage"] = "Claim rejected.";
            return RedirectToAction(nameof(ViewAll));
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
                .Where(c => c.Status == "Pending")
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(pendingClaims);
        }
    }
}