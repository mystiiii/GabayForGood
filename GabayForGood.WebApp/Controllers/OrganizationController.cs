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
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var project = await context.Projects.FindAsync(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Index");
                }

                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID != project.OrganizationId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit this project.";
                    return RedirectToAction("Index");
                }

                var projectVM = mapper.Map<ProjectVM>(project);
                return View(projectVM);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the project.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProjectVM model)
        {
            if (id != model.ProjectId)
            {
                TempData["ErrorMessage"] = "Invalid project ID.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var project = await context.Projects.FindAsync(id);
                    if (project == null)
                    {
                        TempData["ErrorMessage"] = "Project not found.";
                        return RedirectToAction("Index");
                    }

                    var user = await userManager.GetUserAsync(User);
                    if (user?.OrganizationID != project.OrganizationId)
                    {
                        TempData["ErrorMessage"] = "You don't have permission to edit this project.";
                        return RedirectToAction("Index");
                    }

                    project.Description = model.Description;
                    project.GoalAmount = model.GoalAmount;
                    project.StartDate = model.StartDate;
                    project.EndDate = model.EndDate;
                    project.Status = model.Status;
                    project.ModifiedAt = DateTime.UtcNow;

                    context.Projects.Update(project);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Project updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating the project.";
                }
            }

            return View(model);
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

        [Authorize(Roles = "Organization")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var project = await context.Projects.FindAsync(id);
                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Index");
                }

                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID != project.OrganizationId)
                {
                    TempData["ErrorMessage"] = "You don't have permission to delete this project.";
                    return RedirectToAction("Index");
                }

                var projectUpdates = await context.ProjectUpdates
                    .Where(u => u.ProjectId == id)
                    .ToListAsync();

                if (projectUpdates.Any())
                {
                    context.ProjectUpdates.RemoveRange(projectUpdates);
                }

                context.Projects.Remove(project);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Project '{project.Title}' has been deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the project. Please try again.";
                return RedirectToAction("Index");
            }
        }
    }
}
