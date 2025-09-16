using CMCS_ST10445830.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CMCS_ST10445830.Controllers
{
    public class ManageApprovalsController : Controller
    {
        // Mock data for demo (replace with database later)
        private static List<Claim> _claims = new List<Claim>
        {
            new Claim { Id = 1, HoursWorked = 10, HourlyRate = 200, Notes = "Tutorials", Status = "Pending" },
            new Claim { Id = 2, HoursWorked = 8, HourlyRate = 220, Notes = "Marking", Status = "Pending" },
            new Claim { Id = 3, HoursWorked = 12, HourlyRate = 180, Notes = "Lectures", Status = "Approved" }
        };

        // GET: ManageApprovals
        public IActionResult Index()
        {
            return View(_claims);
        }

        // POST: Approve claim
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Approved";
            }
            return RedirectToAction("Index");
        }

        // POST: Reject claim
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected";
            }
            return RedirectToAction("Index");
        }
    }
}
