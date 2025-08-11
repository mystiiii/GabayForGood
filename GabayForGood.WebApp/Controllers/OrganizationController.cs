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
            var user = await userManager.GetUserAsync(User);
            var orgId = user.OrganizationID; 

            var projs = await context.Projects
                .Where(p => p.OrganizationId == orgId)
                .ToListAsync();

            return View(mapper.Map<List<ProjectVM>>(projs));
        }


        [Authorize(Roles = "Organization")]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Organization")]
        public IActionResult Edit()
        {
            return View();
        }

        public async Task<IActionResult> Updates(int id)
        {
            try
            {
                var project = await context.Projects.FindAsync(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Index", "Organization");
                }

                var updates = await context.ProjectUpdates
                    .Where(u => u.ProjectId == id)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                var projectVM = new ProjectVM
                {
                    ProjectId = project.ProjectId,
                    Title = project.Title,
                    Description = project.Description,
                    Category = project.Category,
                    Cause = project.Cause,
                    Location = project.Location,
                    GoalAmount = project.GoalAmount,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Status = project.Status
                };

                var updatesVM = updates.Select(u => new ProjectUpdateVM
                {
                    ProjectUpdateId = u.ProjectUpdateId,
                    ProjectId = u.ProjectId,
                    Title = u.Title,
                    Description = u.Description,
                    CreatedAt = u.CreatedAt
                }).ToList();

                var viewModel = new ProjectUpdatesVM
                {
                    Project = projectVM,
                    Updates = updatesVM
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the project updates.";
                return RedirectToAction("Index", "Organization");
            }
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
