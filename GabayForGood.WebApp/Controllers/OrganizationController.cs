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

        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> Updates(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var project = await context.Projects.FindAsync(id);
                if (project == null || project.OrganizationId != user.OrganizationID)
                {
                    TempData["ErrorMessage"] = "Project not found or you don't have permission to view it.";
                    return RedirectToAction("Index");
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
                    CurrentAmount = project.CurrentAmount,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Status = project.Status,
                    ImageUrl = project.ImageUrl,
                    OrganizationId = project.OrganizationId
                };

                var viewModel = new ProjectUpdatesVM
                {
                    Project = projectVM,
                    Updates = updates
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the project updates.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpGet]
        public async Task<IActionResult> CreateUpdate(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var project = await context.Projects
                    .FirstOrDefaultAsync(p => p.ProjectId == id && p.OrganizationId == user.OrganizationID.Value);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found or you don't have permission to add updates.";
                    return RedirectToAction("Index");
                }

                var model = new CreateProjectUpdateVM
                {
                    ProjectId = id
                };

                return View("AddUpdate", model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the page.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUpdate(CreateProjectUpdateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("AddUpdate", model);
            }

            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return View("AddUpdate", model);
                }

                var project = await context.Projects
                    .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.OrganizationId == user.OrganizationID.Value);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found or you don't have permission to add updates.";
                    return View("AddUpdate", model);
                }

                // Handle image upload if provided
                string imageUrl = model.ImageUrl;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageUrl = await ProcessImageUpload(model.ImageFile);
                    if (imageUrl == null)
                    {
                        return View("AddUpdate", model);
                    }
                }

                var projectUpdate = new ProjectUpdate
                {
                    ProjectId = model.ProjectId,
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.Now
                };

                context.ProjectUpdates.Add(projectUpdate);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Project update added successfully!";
                return RedirectToAction("Updates", new { id = model.ProjectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the update.";
                return View("AddUpdate", model);
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpGet]
        public async Task<IActionResult> EditUpdate(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var update = await context.ProjectUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.ProjectUpdateId == id && u.Project.OrganizationId == user.OrganizationID.Value);

                if (update == null)
                {
                    TempData["ErrorMessage"] = "Update not found or you don't have permission to edit it.";
                    return RedirectToAction("Index");
                }

                var model = new EditProjectUpdateVM
                {
                    ProjectUpdateId = update.ProjectUpdateId,
                    ProjectId = update.ProjectId,
                    Title = update.Title,
                    Description = update.Description,
                    ImageUrl = update.ImageUrl,
                    CreatedAt = update.CreatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the update.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUpdate(EditProjectUpdateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return View(model);
                }

                var update = await context.ProjectUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.ProjectUpdateId == model.ProjectUpdateId && u.Project.OrganizationId == user.OrganizationID.Value);

                if (update == null)
                {
                    TempData["ErrorMessage"] = "Update not found or you don't have permission to edit it.";
                    return View(model);
                }

                // Handle image upload if provided
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(update.ImageUrl))
                    {
                        DeleteImage(update.ImageUrl);
                    }

                    var newImageUrl = await ProcessImageUpload(model.ImageFile);
                    if (newImageUrl != null)
                    {
                        update.ImageUrl = newImageUrl;
                    }
                    else
                    {
                        return View(model);
                    }
                }
                else if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    update.ImageUrl = model.ImageUrl;
                }

                update.Title = model.Title;
                update.Description = model.Description;

                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Project update updated successfully!";
                return RedirectToAction("Updates", new { id = update.ProjectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the project update.";
                return View(model);
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpGet]
        public async Task<IActionResult> DeleteUpdate(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var update = await context.ProjectUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.ProjectUpdateId == id && u.Project.OrganizationId == user.OrganizationID.Value);

                if (update == null)
                {
                    TempData["ErrorMessage"] = "Update not found or you don't have permission to delete it.";
                    return RedirectToAction("Index");
                }

                return View(update);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the update.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUpdateConfirmed(int id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var update = await context.ProjectUpdates
                    .Include(u => u.Project)
                    .FirstOrDefaultAsync(u => u.ProjectUpdateId == id && u.Project.OrganizationId == user.OrganizationID.Value);

                if (update == null)
                {
                    TempData["ErrorMessage"] = "Update not found or you don't have permission to delete it.";
                    return RedirectToAction("Index");
                }

                int projectId = update.ProjectId;

                // Delete associated image
                if (!string.IsNullOrEmpty(update.ImageUrl))
                {
                    DeleteImage(update.ImageUrl);
                }

                context.ProjectUpdates.Remove(update);
                await context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Project update deleted successfully!";
                return RedirectToAction("Updates", new { id = projectId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the update.";
                return RedirectToAction("Index");
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

        // Add these methods to your OrganizationController class

        [Authorize(Roles = "Organization")]
        [HttpGet]
        public async Task<IActionResult> EditProfileOrg()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return RedirectToAction("Index");
                }

                var organization = await context.Organizations.FindAsync(user.OrganizationID.Value);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "Organization not found.";
                    return RedirectToAction("Index");
                }

                var orgVM = mapper.Map<OrgVM>(organization);
                return View(orgVM);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading your profile.";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileOrg(OrgVM model, string CurrentPassword = "", string NewPassword = "", string ConfirmPassword = "")
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user?.OrganizationID == null)
                {
                    TempData["ErrorMessage"] = "Unable to identify your organization.";
                    return View(model);
                }

                var organization = await context.Organizations.FindAsync(user.OrganizationID.Value);
                if (organization == null)
                {
                    TempData["ErrorMessage"] = "Organization not found.";
                    return View(model);
                }

                // Update organization details
                organization.Name = model.Name;
                organization.Description = model.Description;
                organization.YearFounded = model.YearFounded;
                organization.Address = model.Address;
                organization.ContactNo = long.Parse(model.ContactNo);
                organization.ContactPerson = model.ContactPerson;
                organization.OrgLink = model.OrgLink;

                // Handle password change if provided
                bool passwordChanged = false;
                if (!string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword))
                {
                    if (NewPassword != ConfirmPassword)
                    {
                        ModelState.AddModelError("", "New password and confirmation password do not match.");
                        return View(model);
                    }

                    var passwordCheck = await userManager.CheckPasswordAsync(user, CurrentPassword);
                    if (!passwordCheck)
                    {
                        ModelState.AddModelError("", "Current password is incorrect.");
                        return View(model);
                    }

                    var passwordChangeResult = await userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
                    if (!passwordChangeResult.Succeeded)
                    {
                        foreach (var error in passwordChangeResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }

                    // Update organization password as well
                    organization.Password = NewPassword;
                    passwordChanged = true;
                }

                // Save organization changes
                context.Organizations.Update(organization);
                await context.SaveChangesAsync();

                string successMessage = "Profile updated successfully!";
                if (passwordChanged)
                {
                    successMessage += " Password has been changed.";
                }

                TempData["SuccessMessage"] = successMessage;
                return RedirectToAction("Index", "Organization");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                return View(model);
            }
        }
    }
}