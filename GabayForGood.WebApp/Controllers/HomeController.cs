using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GabayForGood.WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
<<<<<<< HEAD

    public IActionResult Privacy()
    {
        return View();
    }

    //pollo - landing page
    public IActionResult Landing()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
=======
>>>>>>> master
}
