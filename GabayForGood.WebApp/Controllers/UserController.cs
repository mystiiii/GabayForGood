using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;

namespace GabayForGood.WebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly AppDbContext context;
        private readonly IMapper mapper;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AppDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
            this.mapper = mapper;
        }

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
                Email = vm.Email,
                UserName = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                ContactNo = vm.ContactNo,
                FullName = $"{vm.FirstName} {vm.LastName}",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");

                await userManager.AddClaimsAsync(user, new[]
                {
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? "")
        });

                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Browse", "User");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        [HttpGet]
        public IActionResult SignIn(string? returnUrl = null)
        {
            return View(new SignInVM { ReturnUrl = returnUrl ?? Url.Content("~/") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInVM svm)
        {
            if (!ModelState.IsValid)
                return View(svm);

            var user = await userManager.FindByNameAsync(svm.Email);

            if (user != null)
            {
                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    ModelState.AddModelError("", "Admin users cannot login through this form. Please use the admin portal.");
                    return View(svm);
                }

                var result = await signInManager.PasswordSignInAsync(svm.Email, svm.Password, false, false);
                if (result.Succeeded)
                {
                    if (await userManager.IsInRoleAsync(user, "Organization"))
                    {
                        return RedirectToAction("Index", "Organization");
                    }
                    else if (await userManager.IsInRoleAsync(user, "User"))
                    {
                        return RedirectToAction("Browse", "User");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(svm);
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
        public async Task<IActionResult> Browse()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            var projects = await context.Projects
                .Where(p => p.Status == "Active")
                .Include(p => p.Organization)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var projectVMs = mapper.Map<List<ProjectVM>>(projects);
            return View(projectVMs);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn");

            var vm = new EditProfileVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ContactNo = user.ContactNo
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn");

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Email = vm.Email;
            user.UserName = vm.Email;
            user.ContactNo = vm.ContactNo;
            user.FullName = $"{vm.FirstName} {vm.LastName}";

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await userManager.RemoveClaimsAsync(user, new[]
                {
                    new Claim("FirstName", user.FirstName ?? ""),
                    new Claim("LastName", user.LastName ?? "")
                });
                await userManager.AddClaimsAsync(user, new[]
                {
                    new Claim("FirstName", user.FirstName ?? ""),
                    new Claim("LastName", user.LastName ?? "")
                });

                await signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("Browse");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        [Authorize]
        [HttpGet("/Donation/User/{id}")]
        public async Task<IActionResult> Donate(int id)
        {
            try
            {
                var project = await context.Projects
                    .Include(p => p.Organization) // Include organization data
                    .FirstOrDefaultAsync(p => p.ProjectId == id);

                if (project == null)
                {
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Browse");
                }

                if (project.Status != "Active")
                {
                    TempData["ErrorMessage"] = "This project is no longer accepting donations.";
                    return RedirectToAction("Browse");
                }

                var currentAmount = await context.Donations
                    .Where(d => d.ProjectId == id && d.Status == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0;

                var fundingPercentage = project.GoalAmount > 0
                    ? (currentAmount / project.GoalAmount) * 100
                    : 0;

                var daysRemaining = (project.EndDate - DateTime.Now).Days;

                var donationVM = new DonationVM
                {
                    ProjectId = id,
                    Project = mapper.Map<ProjectVM>(project),
                    Organization = mapper.Map<OrgVM>(project.Organization), // Map organization data
                    CurrentAmount = currentAmount,
                    FundingPercentage = Math.Min(fundingPercentage, 100),
                    DaysRemaining = Math.Max(daysRemaining, 0)
                };

                return View("Donation", donationVM);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while loading the donation page.";
                return RedirectToAction("Browse");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDonation(DonationVM model)
        {
            // Remove model state validation for nested properties that aren't being submitted
            ModelState.Remove("Project.Cause");
            ModelState.Remove("Project.Title");
            ModelState.Remove("Project.Status");
            ModelState.Remove("Project.EndDate");
            ModelState.Remove("Project.Category");
            ModelState.Remove("Project.Location");
            ModelState.Remove("Project.GoalAmount");
            ModelState.Remove("Project.Description");
            ModelState.Remove("Project.StartDate");
            ModelState.Remove("Organization.Name");
            ModelState.Remove("Organization.Description");
            ModelState.Remove("Organization.YearFounded");
            ModelState.Remove("Organization.Address");
            ModelState.Remove("Organization.Email");
            ModelState.Remove("Organization.ContactNo");
            ModelState.Remove("Organization.ContactPerson");
            ModelState.Remove("Organization.OrgLink");

            if (!ModelState.IsValid)
            {
                var project = await context.Projects
                    .Include(p => p.Organization)
                    .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

                if (project != null)
                {
                    model.Project = mapper.Map<ProjectVM>(project);
                    model.Organization = mapper.Map<OrgVM>(project.Organization);
                    var currentAmount = await context.Donations
                        .Where(d => d.ProjectId == model.ProjectId && d.Status == "Completed")
                        .SumAsync(d => (decimal?)d.Amount) ?? 0;
                    model.CurrentAmount = currentAmount;
                    model.FundingPercentage = project.GoalAmount > 0 ? (currentAmount / project.GoalAmount) * 100 : 0;
                    model.DaysRemaining = Math.Max((project.EndDate - DateTime.Now).Days, 0);
                }
                return View("Donation", model);
            }
            else
            {
                var project = await context.Projects
                    .Include(p => p.Organization)
                    .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId);

                if (project == null || project.Status != "Active")
                {
                    TempData["ErrorMessage"] = "Project not found or inactive.";
                    return RedirectToAction("Browse");
                }

                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to make a donation.";
                    return RedirectToAction("SignIn", "User");
                }

                System.Diagnostics.Debug.WriteLine($"UserManager UserId: {currentUser.Id}");

                var donation = new Donation
                {
                    UserId = currentUser.Id,
                    ProjectId = model.ProjectId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    Message = model.Message ?? "",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    await context.Donations.AddAsync(donation);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                    throw;
                }

                bool paymentSuccess = true;
                string paymentMessage = "Payment processed successfully.";

                donation.Status = paymentSuccess ? "Completed" : "Failed";
                context.Donations.Update(donation);

                if (paymentSuccess)
                {
                    var updatedCurrentAmount = await context.Donations
                        .Where(d => d.ProjectId == model.ProjectId && d.Status == "Completed")
                        .SumAsync(d => (decimal?)d.Amount) ?? 0;

                    project.CurrentAmount = updatedCurrentAmount + project.CurrentAmount;
                    context.Projects.Update(project);
                }

                await context.SaveChangesAsync();

                if (paymentSuccess)
                {
                    var updatedCurrentAmount = await context.Donations
                        .Where(d => d.ProjectId == model.ProjectId && d.Status == "Completed")
                        .SumAsync(d => (decimal?)d.Amount) ?? 0;

                    project.CurrentAmount = updatedCurrentAmount;
                    context.Projects.Update(project);
                    await context.SaveChangesAsync();
                }

                if (paymentSuccess)
                {
                    TempData["SuccessMessage"] = "Thank you for your donation!";
                    return RedirectToAction("Browse", "User");
                }
                else
                {
                    TempData["ErrorMessage"] = "Payment failed. Please try again.";
                    return RedirectToAction("Donate", new { id = model.ProjectId });
                }
            }
        }

        public async Task<IActionResult> ProjectUpdates(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ProjectUpdates called with id: {id}");

                // Step 1: Check if project exists (removed unnecessary includes for string properties)
                var project = await context.Projects
                    .Include(p => p.Organization) // Only include Organization since Category and Cause are strings
                    .FirstOrDefaultAsync(p => p.ProjectId == id);

                System.Diagnostics.Debug.WriteLine($"Project found: {project != null}");

                if (project == null)
                {
                    System.Diagnostics.Debug.WriteLine("Project is null, redirecting to Browse");
                    TempData["ErrorMessage"] = "Project not found.";
                    return RedirectToAction("Browse", "User");
                }

                System.Diagnostics.Debug.WriteLine($"Project details - ID: {project.ProjectId}, Title: {project.Title}");

                // Step 2: Get project updates
                var updates = await context.ProjectUpdates
                    .Where(u => u.ProjectId == id)
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Found {updates.Count} updates for project {id}");

                // Step 3: Create ProjectVM - Category and Cause are strings, not navigation properties
                var projectVM = new ProjectVM
                {
                    ProjectId = project.ProjectId,
                    Title = project.Title ?? "No Title",
                    Description = project.Description ?? "No Description",
                    GoalAmount = project.GoalAmount,
                    CurrentAmount = project.CurrentAmount,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    ImageUrl = project.ImageUrl ?? "",
                    Location = project.Location ?? "No Location",
                    // Fixed: Category and Cause are strings, not navigation properties
                    Category = project.Category ?? "Unknown",
                    Cause = project.Cause ?? "Unknown",
                    Status = project.Status ?? "Active",
                    OrganizationName = project.Organization?.Name ?? "Unknown",
                    OrganizationId = project.OrganizationId,
                    CreatedAt = project.CreatedAt,
                    ModifiedAt = project.ModifiedAt
                };

                System.Diagnostics.Debug.WriteLine("ProjectVM created successfully");

                // Step 4: Create view model
                var viewModel = new ProjectUpdatesVM
                {
                    Project = projectVM,
                    Updates = updates
                };

                System.Diagnostics.Debug.WriteLine("View model created successfully");

                // Set page title
                ViewData["Title"] = $"Updates - {project.Title}";

                System.Diagnostics.Debug.WriteLine("Returning view");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Enhanced logging with full exception details
                System.Diagnostics.Debug.WriteLine($"Exception in ProjectUpdates:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");

                TempData["ErrorMessage"] = $"An error occurred while loading the project updates: {ex.Message}";
                return RedirectToAction("Browse", "User");
            }
        }
    }
}