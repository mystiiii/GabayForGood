using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GabayForGood.WebApp.Controllers;

public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            await HttpContext.SignOutAsync(); // Prevent Browse from appearing by default if still signed in
        }

        return View();
    }
}