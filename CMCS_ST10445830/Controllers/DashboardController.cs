using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult LecturerDashboard()
        {
            return View();
        }
        public IActionResult CoordinatorDashboard()
        {
            return View();
        }
    }
}
