using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;
using CMCS_ST10445830.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CMCS_ST10445830.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRManagementController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<HRManagementController> _logger;

        public HRManagementController(AuthDbContext context, ILogger<HRManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: HRManagement - List all users
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserProfile)
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.Username)
                    .ToListAsync();

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for HR management");

                // Fallback: load users without profiles
                var users = await _context.Users
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.Username)
                    .ToListAsync();

                return View(users);
            }
        }

        // GET: HRManagement/Create - Create new user
        public IActionResult Create()
        {
            return View();
        }

        // POST: HRManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string FirstName, string LastName, string Email, decimal HourlyRate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username already exists
                    if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                        return View(user);
                    }

                    // Create user
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create user profile
                    var userProfile = new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = FirstName,
                        LastName = LastName,
                        Email = Email,
                        HourlyRate = HourlyRate
                    };

                    _context.UserProfiles.Add(userProfile);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"User {user.Username} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    ModelState.AddModelError("", "An error occurred while creating the user.");
                }
            }

            return View(user);
        }

        // GET: HRManagement/Edit/5 - Edit user
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: HRManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string FirstName, string LastName, string Email, decimal HourlyRate, string? PasswordHash = null)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing user from database to preserve password if not changed
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update user properties
                    existingUser.Username = user.Username;
                    existingUser.Role = user.Role;

                    // Only update password if a new one was provided
                    if (!string.IsNullOrEmpty(PasswordHash))
                    {
                        existingUser.PasswordHash = PasswordHash;
                    }

                    _context.Users.Update(existingUser);

                    // Update or create user profile
                    var existingProfile = await _context.UserProfiles.FindAsync(id);
                    if (existingProfile != null)
                    {
                        existingProfile.FirstName = FirstName;
                        existingProfile.LastName = LastName;
                        existingProfile.Email = Email;
                        existingProfile.HourlyRate = HourlyRate;
                        _context.UserProfiles.Update(existingProfile);
                    }
                    else
                    {
                        var userProfile = new UserProfile
                        {
                            UserId = id,
                            FirstName = FirstName,
                            LastName = LastName,
                            Email = Email,
                            HourlyRate = HourlyRate
                        };
                        _context.UserProfiles.Add(userProfile);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User {user.Username} updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user");
                    ModelState.AddModelError("", "An error occurred while updating the user.");
                }
            }

            // Reload the user with profile for the view
            user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            return View(user);
        }

        // GET: HRManagement/Reports - Generate reports
        public async Task<IActionResult> Reports()
        {
            try
            {
                var reportData = await _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.UserProfile)
                    .Include(c => c.Coordinator)
                    .ThenInclude(c => c.UserProfile)
                    .Where(c => c.Status == "Approved")
                    .OrderByDescending(c => c.ReviewedAt)
                    .ToListAsync();

                return View(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                // Fallback: load claims without user profiles
                var reportData = await _context.Claims
                    .Where(c => c.Status == "Approved")
                    .OrderByDescending(c => c.ReviewedAt)
                    .ToListAsync();

                return View(reportData);
            }
        }
        [HttpPost]
        public async Task<IActionResult> GenerateInvoice(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var claimsQuery = _context.Claims
                    .Include(c => c.Lecturer)
                    .ThenInclude(l => l.UserProfile)
                    .Include(c => c.Coordinator)
                    .ThenInclude(c => c.UserProfile)
                    .Where(c => c.Status == "Approved");

                if (startDate.HasValue)
                {
                    claimsQuery = claimsQuery.Where(c => c.SubmittedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    claimsQuery = claimsQuery.Where(c => c.SubmittedAt <= endDate.Value);
                }

                var approvedClaims = await claimsQuery
                    .OrderBy(c => c.Lecturer.UserProfile.LastName)
                    .ThenBy(c => c.SubmittedAt)
                    .ToListAsync();

                if (!approvedClaims.Any())
                {
                    TempData["ErrorMessage"] = "No approved claims found for the selected date range.";
                    return RedirectToAction(nameof(Reports));
                }

                // Generate PDF
                var pdfService = new PdfReportService();
                var pdfBytes = pdfService.GenerateClaimsReport(approvedClaims, startDate, endDate);

                var fileName = $"Claims_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                TempData["ErrorMessage"] = "Error generating PDF report. Please try again.";
                return RedirectToAction(nameof(Reports));
            }
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}