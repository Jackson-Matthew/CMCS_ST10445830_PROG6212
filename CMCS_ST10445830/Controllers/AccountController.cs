using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "lecturer" && password == "password")
            {
                return RedirectToAction("LecturerDashboard", "Dashboard");
            }
            else if(username == "coordinator" && password == "password")
            {
                return RedirectToAction("CoordinatorDashboard", "Dashboard");
            }
            
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        public IActionResult Logout()
        {
            return RedirectToAction("Index","Home");
        }
    }
}
