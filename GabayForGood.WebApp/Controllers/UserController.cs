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

            // Get all active projects with their organization details
            var projects = await context.Projects
                .Where(p => p.Status == "Active")
                .Include(p => p.Organization) // Include organization details if needed
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var projectVMs = mapper.Map<List<ProjectVM>>(projects);
            return View(projectVMs);
        }

        // EDIT PROFILE
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
                // Update claims with new name
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
                var project = await context.Projects.FindAsync(id);
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

                // Calculate current donation amount for this project
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
                    CurrentAmount = currentAmount,
                    FundingPercentage = Math.Min(fundingPercentage, 100), // Cap at 100%
                    DaysRemaining = Math.Max(daysRemaining, 0) // Prevent negative days
                };

                // Explicitly load the view from Views/User/Donation.cshtml
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
            if (!ModelState.IsValid)
            {
                // Reload the donation page if invalid
                var project = await context.Projects.FindAsync(model.ProjectId);
                if (project != null)
                {
                    model.Project = mapper.Map<ProjectVM>(project);
                    var currentAmount = await context.Donations
                        .Where(d => d.ProjectId == model.ProjectId && d.Status == "Completed")
                        .SumAsync(d => (decimal?)d.Amount) ?? 0;
                    model.CurrentAmount = currentAmount;
                    model.FundingPercentage = project.GoalAmount > 0 ? (currentAmount / project.GoalAmount) * 100 : 0;
                    model.DaysRemaining = Math.Max((project.EndDate - DateTime.Now).Days, 0);
                }
                return View("Donation", model);
            }

            try
            {
                // Verify project
                var project = await context.Projects.FindAsync(model.ProjectId);
                if (project == null || project.Status != "Active")
                {
                    TempData["ErrorMessage"] = "Project not found or inactive.";
                    return RedirectToAction("Browse");
                }

                // Get user
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to donate.";
                    return RedirectToAction("SignIn");
                }

                // Create donation entry
                var donation = new Donation
                {
                    UserId = user.Id,
                    ProjectId = model.ProjectId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    Message = model.Message ?? "",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await context.Donations.AddAsync(donation);
                await context.SaveChangesAsync();

                // TEMP: Mark as success until real payment gateway is integrated
                bool paymentSuccess = true;
                string paymentMessage = "Payment processed successfully.";

                // Update donation status
                donation.Status = paymentSuccess ? "Completed" : "Failed";
                context.Donations.Update(donation);
                await context.SaveChangesAsync();

                if (paymentSuccess)
                {
                    TempData["SuccessMessage"] = $"Thank you for donating ₱{model.Amount:N2}! {paymentMessage}";
                    return RedirectToAction("DonationSuccess", new { donationId = donation.DonationId });
                }
                else
                {
                    TempData["ErrorMessage"] = paymentMessage;
                    return RedirectToAction("Donation", new { id = model.ProjectId });
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while processing your donation.";
                return RedirectToAction("Donation", new { id = model.ProjectId });
            }
        }
    }
}