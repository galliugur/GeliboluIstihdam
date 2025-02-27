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
            // Kullanýcý doðrulama iþlemleri burada yapýlýr.
            // Örnek kullanýcý türüne göre ayýrma:
            if (email == "isveren@example.com") // Bu bir iþveren ise
            {
                return RedirectToAction("Index", "Home", new { area = "Employer" });
            }
            else if (email == "isarayan@example.com") // Bu bir iþ arayan ise
            {
                return RedirectToAction("Index", "Home", new { area = "Employee" });
            }

            // Kullanýcý bilgileri yanlýþ ise ana sayfaya döndür
            ViewBag.ErrorMessage = "Geçersiz giriþ bilgileri.";
            return View("Index");
        }

    }
}
