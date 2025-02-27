using Microsoft.AspNetCore.Mvc;

namespace GeliboluIstihdam.Areas.Employee.Controllers
{
    public class MainController : Controller
    {
        [Area("Employee")]

        public IActionResult Index()
        {
            return View();
        }
    }
}
