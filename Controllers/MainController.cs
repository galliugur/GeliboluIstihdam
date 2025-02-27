using Microsoft.AspNetCore.Mvc;

namespace GeliboluIstihdam.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
