using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GabayForGood.WebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Landing()
    {
        return View();
    }
}
