using GeliboluIstihdam.Models;
using Microsoft.AspNetCore.Mvc;
using Netia.Katharsys.Core;
using System.Data;
using System.Diagnostics;

namespace GeliboluIstihdam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Kullan�c� do�rulama i�lemleri burada yap�l�r.
            // �rnek kullan�c� t�r�ne g�re ay�rma:
            if (email == "isveren@example.com") // Bu bir i�veren ise
            {
                return RedirectToAction("Index", "Home", new { area = "Employer" });
            }
            else if (email == "isarayan@example.com") // Bu bir i� arayan ise
            {
                return RedirectToAction("Index", "Home", new { area = "Employee" });
            }

            // Kullan�c� bilgileri yanl�� ise ana sayfaya d�nd�r
            ViewBag.ErrorMessage = "Ge�ersiz giri� bilgileri.";
            return View("Index");
        }

    }
}
