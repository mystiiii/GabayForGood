using Microsoft.AspNetCore.Identity;
using GabayForGood.DataModel;

namespace GabayForGood.WebApp.Middleware
{
    public class RoleBasedDefaultRouteMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RoleBasedDefaultRouteMiddleware> logger;

        public RoleBasedDefaultRouteMiddleware(RequestDelegate next, ILogger<RoleBasedDefaultRouteMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/" && context.Request.Method == "GET")
            {
                logger.LogInformation("Processing root path request");

                var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

                if (signInManager.IsSignedIn(context.User))
                {
                    logger.LogInformation("User is signed in, checking roles");

                    var user = await userManager.GetUserAsync(context.User);
                    if (user != null)
                    {
                        logger.LogInformation($"Found user: {user.Email}");

                        var userRoles = await userManager.GetRolesAsync(user);
                        logger.LogInformation($"User roles: {string.Join(", ", userRoles)}");

                        if (await userManager.IsInRoleAsync(user, "Admin"))
                        {
                            logger.LogInformation("Redirecting to Admin");
                            context.Response.Redirect("/Admin");
                            return;
                        }
                        else if (await userManager.IsInRoleAsync(user, "Organization"))
                        {
                            logger.LogInformation("Redirecting to Organization");
                            context.Response.Redirect("/Organization");
                            return;
                        }
                        else if (await userManager.IsInRoleAsync(user, "User"))
                        {
                            logger.LogInformation("Redirecting to User/Browse");
                            context.Response.Redirect("/User/Browse"); // More specific redirect
                            return;
                        }
                        else
                        {
                            logger.LogWarning($"User {user.Email} has no roles assigned");
                        }
                    }
                    else
                    {
                        logger.LogWarning("User is signed in but user object is null");
                    }
                }
                else
                {
                    logger.LogInformation("User is not signed in, continuing to home page");
                }
            }

            await next(context);
        }
    }
}