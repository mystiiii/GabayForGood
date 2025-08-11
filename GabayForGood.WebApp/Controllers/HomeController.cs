using GabayForGood.DataModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GabayForGood.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;

    public HomeController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                if (await userManager.IsInRoleAsync(user, "Organization"))
                {
                    return RedirectToAction("Index", "Organization");
                }
                else if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (await userManager.IsInRoleAsync(user, "User"))
                {
                    return RedirectToAction("Index", "User");
                }
            }
        }
        return View();
    }
}