using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        // Dashboard
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            return View();
        }

        // Players Management
        public IActionResult Players()
        {
            ViewData["Title"] = "Players Management";
            return View();
        }

        // Items & Shop
        public IActionResult Items()
        {
            ViewData["Title"] = "Items & Shop";
            return View();
        }

        // Transactions
        public IActionResult Transactions()
        {
            ViewData["Title"] = "Transactions";
            return View();
        }

        // Resources
        public IActionResult Resources()
        {
            ViewData["Title"] = "Game Resources";
            return View();
        }

        // API Documentation
        public IActionResult ApiDocs()
        {
            ViewData["Title"] = "API Documentation";
            return View();
        }
    }
}
