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
        private readonly IWebHostEnvironment webHostEnvironment;

        public OrganizationController(AppDbContext context, IMapper mapper, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.mapper = mapper;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
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
        [HttpPost]
        public async Task<IActionResult> Add(ProjectVM model)
        {
            // Remove ImageUrl from ModelState validation since it's generated automatically
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await userManager.GetUserAsync(User);
                    if (user == null || !user.OrganizationID.HasValue)
                    {
                        TempData["ErrorMessage"] = "Unable to identify your organization.";
                        return View(model);
                    }

                    // Handle image upload
                    string imageUrl = null;
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        imageUrl = await ProcessImageUpload(model.ImageFile);
                        if (imageUrl == null)
                        {
                            // Error occurred during upload, ModelState should contain the error
                            return View(model);
                        }
                    }

                    var projectModel = mapper.Map<Project>(model);
                    projectModel.CreatedAt = DateTime.UtcNow;
                    projectModel.OrganizationId = user.OrganizationID.Value;
                    projectModel.ImageUrl = imageUrl; 

                    await context.Projects.AddAsync(projectModel);
                    await context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Project created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while creating the project.";
                }
            }

            return View(model);
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

            // Remove ImageUrl and ImageFile from ModelState validation since they're optional
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ImageFile");

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

                    // Handle image upload if new image is provided
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(project.ImageUrl))
                        {
                            DeleteImage(project.ImageUrl);
                        }

                        // Upload new image
                        var newImageUrl = await ProcessImageUpload(model.ImageFile);
                        if (newImageUrl != null)
                        {
                            project.ImageUrl = newImageUrl;
                        }
                        else
                        {
                            return View(model); // Error occurred during upload
                        }
                    }
                    project.Description = model.Description;
                    project.GoalAmount = model.GoalAmount;
                    project.CurrentAmount = model.CurrentAmount; // Add this line since it's editable in the form
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

                // Delete associated image
                if (!string.IsNullOrEmpty(project.ImageUrl))
                {
                    DeleteImage(project.ImageUrl);
                }

                // Delete project updates
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

        #region Private Helper Methods

        private async Task<string> ProcessImageUpload(IFormFile imageFile)
        {
            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Only JPG, JPEG, PNG, GIF, and WebP files are allowed.");
                    return null;
                }

                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                    return null;
                }

                var uniqueFileName = Guid.NewGuid().ToString() + extension;

                var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "projects");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"/uploads/projects/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ImageFile", "An error occurred while uploading the image.");
                return null;
            }
        }

        private void DeleteImage(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                var fullPath = Path.Combine(webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}