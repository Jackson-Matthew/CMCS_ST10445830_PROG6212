using CMCS_ST10445830.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS_ST10445830.Controllers
{
    public class UploadController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Upload model)
        {
            if (ModelState.IsValid)
            {
                TempData["Message"] = "File uploaded successfully.";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
