using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GabayForGood.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // SIGN UP
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                FullName = "User"
            };

            var result = await userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Browse", "User");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        // SIGN IN
        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            return View(new SignInVM { ReturnUrl = returnUrl ?? Url.Content("~/") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await signInManager.PasswordSignInAsync(vm.UserName, vm.Password, false, false);

            if (result.Succeeded)
                return RedirectToAction("Browse", "User");

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Browse()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }
    }
}
