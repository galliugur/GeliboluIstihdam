using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using GeliboluIstihdam.Areas.Employer.Models;

namespace GeliboluIstihdam.Areas.Employer.Controllers
{
    [Area("Employer")]
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            ViewData["PageTitle"] = "Dashboard";
            ViewData["Page"] = "Dashboard";
            return View();
        }
        public IActionResult Profile()
        {
            ViewData["PageTitle"] = "Profil Detayları";
            ViewData["Page"] = "Profil";
            ViewData["SubPage"] = "Profil Detayları";
            return View();
        }

        public IActionResult JobPostings()
        {
            ViewData["PageTitle"] = "İş İlanları";
            ViewData["Page"] = "İş İlanları";

            var model = new JobPostingsViewModel
            {
                JobPostings = new List<JobPosting>() // Veritabanından gelecek veriler
            };

            return View(model);
        }

        public IActionResult CreateJobPosting()
        {
            ViewData["PageTitle"] = "Yeni İş İlanı";
            ViewData["Page"] = "İş İlanları";
            ViewData["SubPage"] = "Yeni İlan";
            return View();
        }

        public IActionResult Applications()
        {
            ViewData["PageTitle"] = "Başvurular";
            ViewData["Page"] = "Başvurular";
            return View();
        }

        public IActionResult CompanyUsers()
        {
            ViewData["PageTitle"] = "Şirket Kullanıcıları";
            ViewData["Page"] = "Firma Yönetimi";
            ViewData["SubPage"] = "Kullanıcılar";
            return View();
        }

        public IActionResult CompanySettings()
        {
            ViewData["PageTitle"] = "Firma Ayarları";
            ViewData["Page"] = "Firma Yönetimi";
            ViewData["SubPage"] = "Ayarlar";
            return View();
        }
    }
}
