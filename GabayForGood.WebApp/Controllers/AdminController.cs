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
                return View(model);
            }

            ApplicationUser admin = await userManager.FindByNameAsync(model.Username);

            if (admin != null)
            {
                var result = await signInManager.PasswordSignInAsync(admin.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnURL)) return LocalRedirect(returnURL);
                    return LocalRedirect("/Admin");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid Credentials");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterOrg(OrgVM org)
        {
            if (!ModelState.IsValid)
            {
                return View(org);
            }

            var orgEntity = mapper.Map<Organization>(org);
            orgEntity.CreatedAt = DateTime.UtcNow;
            orgEntity.Password = "GFGOrg123!";

            await context.Organizations.AddAsync(orgEntity);
            await context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                FullName = "Organization",
                CreatedAt = orgEntity.CreatedAt,
                UserName = orgEntity.Email, 
                Email = orgEntity.Email,
                OrganizationID = orgEntity.OrganizationId 
            };

            var result = await userManager.CreateAsync(user, orgEntity.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Organization");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                context.Organizations.Remove(orgEntity);
                await context.SaveChangesAsync();

                return View(org);
            }

            return RedirectToAction("Index", "Admin");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var org = await context.Organizations.FindAsync(id);
            if (org != null)
            {
                var orgUsers = userManager.Users
                    .Where(u => u.OrganizationID == id)
                    .ToList();

                foreach (var user in orgUsers)
                {
                    await userManager.DeleteAsync(user);
                }

                context.Organizations.Remove(org);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Admin");
        }



        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("LogIn");
        }
    }
}