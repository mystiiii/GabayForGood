using AutoMapper;
using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GabayForGood.WebApp.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly AppDbContext context;

        public OrganizationController(AppDbContext context, IMapper mapper, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> Index()
        {
            var projs = await context.Projects.ToListAsync();
            return View(mapper.Map<List<ProjectVM>>(projs));
        }

        [Authorize(Roles = "Organization")]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        public async Task<IActionResult> Add(ProjectVM model)
        {
            if (ModelState.IsValid)
            {
                var projectModel = mapper.Map<Project>(model);
                projectModel.CreatedAt = DateTime.UtcNow;

                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                if (user.OrganizationID.HasValue)
                {
                    projectModel.OrganizationId = user.OrganizationID.Value;
                } 

                await context.Projects.AddAsync(projectModel);
                await context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

    }
}
