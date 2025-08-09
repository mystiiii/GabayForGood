using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using GabayForGood.DataModel;

namespace GabayForGood.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            string[] roles = { "Admin", "User", "Organization" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@gabayforgood.com";
            var adminPassword = "P@ssw0rd!";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            var organizations = context.Organizations.ToList();
            foreach (var org in organizations)
            {
                var existingOrgUser = await userManager.FindByEmailAsync(org.Email);
                if (existingOrgUser == null)
                {
                    var orgUser = new ApplicationUser
                    {
                        UserName = org.Email,
                        Email = org.Email,
                        FullName = org.Name,
                        EmailConfirmed = true
                    };

                    var password = string.IsNullOrWhiteSpace(org.Password) ? "GFGOrg123!" : org.Password;

                    var createOrgResult = await userManager.CreateAsync(orgUser, password);
                    if (createOrgResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(orgUser, "Organization");
                    }
                }
            }
        }
    }
}
