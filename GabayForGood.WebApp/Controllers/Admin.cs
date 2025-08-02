using Microsoft.AspNetCore.Mvc;

namespace GabayForGood.WebApp.Controllers
{
    public class Admin : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
