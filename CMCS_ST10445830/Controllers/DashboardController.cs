using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Lecturer")]
        public IActionResult LecturerDashboard()
        {
            var username = User.Identity?.Name;
            ViewBag.Username = username;
            ViewBag.Role = "Lecturer";
            return View();
        }

        [Authorize(Roles = "Academic coordinator")]
        public IActionResult CoordinatorDashboard()
        {
            var username = User.Identity?.Name;
            ViewBag.Username = username;
            ViewBag.Role = "Academic Coordinator";
            return View();
        }

        [Authorize(Roles = "HR")]
        public IActionResult HRDashboard() // Changed from ManagerDashboard to HRDashboard
        {
            var username = User.Identity?.Name;
            ViewBag.Username = username;
            ViewBag.Role = "HR Manager";
            return View();
        }
    }
}