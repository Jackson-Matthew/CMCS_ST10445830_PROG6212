using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMCS_ST10445830.Controllers
{
    public class ClaimsController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Claim claim)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save claim to database
                // e.g., _context.Claims.Add(claim); _context.SaveChanges();

                return RedirectToAction("Success");
            }

            return View(claim);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}

