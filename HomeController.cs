using Microsoft.AspNetCore.Mvc;

namespace LehmanCustomConstruction
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
