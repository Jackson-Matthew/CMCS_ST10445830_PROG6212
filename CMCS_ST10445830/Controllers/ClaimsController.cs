using Microsoft.AspNetCore.Mvc;
using CMCS_ST10445830.Models;
using System.Collections.Generic;

namespace CMCS_ST10445830.Controllers
{
    public class ClaimsController : Controller
    {

        public IActionResult ViewAll()
        {
            var claims = new List<Claim>
            {
                new Claim { Id = 1, LecturerName = "Dr. Jackson", HoursWorked = 10, HourlyRate = 200, Notes = "Tutorials", Status = "Pending" },
                new Claim { Id = 2, LecturerName = "Prof. Davids", HoursWorked = 8, HourlyRate = 220, Notes = "Marking", Status = "Approved" },
                new Claim { Id = 3, LecturerName = "Dr. khumalo", HoursWorked = 12, HourlyRate = 180, Notes = "Lectures", Status = "Paid" }
            };

            return View(claims);
        }
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Index()
        {
            var claims = new List<Claim>
            {
                new Claim { Id = 1, HoursWorked = 10, HourlyRate = 200, Notes = "Tutorials", Status = "Pending" },
                new Claim { Id = 2, HoursWorked = 8, HourlyRate = 220, Notes = "Marking", Status = "Approved" },
                new Claim { Id = 3, HoursWorked = 12, HourlyRate = 180, Notes = "Lectures", Status = "Paid" }
            };

            return View(claims);
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

