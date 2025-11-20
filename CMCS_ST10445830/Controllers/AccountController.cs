using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS_ST10445830.Data;
using CMCS_ST10445830.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CMCS_ST10445830.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Find user by username
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.Username);

                    if (user != null)
                    {
                        // Verify password (in real application, use proper password hashing)
                        if (VerifyPassword(model.Password, user.PasswordHash))
                        {
                            // Check if user's role matches the selected role
                            if (user.Role != model.Role)
                            {
                                ModelState.AddModelError(string.Empty, $"Invalid role selection. Your account role is: {user.Role}");
                                return View(model);
                            }

                            // Create claims identity - using fully qualified name
                            var claims = new List<System.Security.Claims.Claim>
                            {
                                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new System.Security.Claims.Claim(ClaimTypes.Name, user.Username),
                                new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
                                new System.Security.Claims.Claim("UserId", user.Id.ToString())
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = model.RememberMe,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                            };

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                authProperties);

                            _logger.LogInformation("User {Username} logged in.", model.Username);

                            // Redirect based on role - FIXED FOR HR
                            return user.Role.ToLower() switch
                            {
                                "lecturer" => RedirectToAction("LecturerDashboard", "Dashboard"),
                                "academic coordinator" => RedirectToAction("CoordinatorDashboard", "Dashboard"),
                                "hr" => RedirectToAction("HRDashboard", "Dashboard"), // Changed from ManagerDashboard to HRDashboard
                                _ => RedirectToAction("Index", "Home")
                            };
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Simple password verification (in production, use proper hashing)
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // For demo purposes - in real application, use proper password hashing like BCrypt
            return enteredPassword == storedHash; // This is NOT secure for production!
        }
    }
}