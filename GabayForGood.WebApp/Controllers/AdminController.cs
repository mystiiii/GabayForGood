using AutoMapper;
using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GabayForGood.WebApp.Controllers
{
    public class AdminController : Controller 
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly AppDbContext context;

        public AdminController(AppDbContext context, IMapper mapper, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult LogIn(string? returnURL)
        {
            AdminVM model = new AdminVM();
            if (!string.IsNullOrEmpty(returnURL)) model.ReturnURL = returnURL;
            return View();
        }

        [Authorize(Roles = "Admin")] 
        public IActionResult RegisterOrg()
        {
            return View();
        }

        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Index()
        {
            var orgs = await context.Organizations.ToListAsync();
            return View(mapper.Map<List<OrgVM>>(orgs));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(AdminVM model, string? returnURL)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please complete all required fields.");
                return View(model);
            }

            ApplicationUser admin = await userManager.FindByEmailAsync(model.Username);
            if (admin != null)
            {
                var result = await signInManager.PasswordSignInAsync(admin, model.Password, false, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnURL)) return LocalRedirect(returnURL);
                    return LocalRedirect("/Admin");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Credentials");
                    return View(model);
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid Credentials");
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add()
        {
            return LocalRedirect("/Admin");
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("LogIn");
        }
    }
}