using Microsoft.AspNetCore.Identity;
using GabayForGood.DataModel;

namespace GabayForGood.WebApp.Middleware
{
    public class RoleBasedDefaultRouteMiddleware
    {
        private readonly RequestDelegate next;

        public RoleBasedDefaultRouteMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/" && context.Request.Method == "GET")
            {
                var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

                if (signInManager.IsSignedIn(context.User))
                {
                    var user = await userManager.GetUserAsync(context.User);
                    if (user != null)
                    {
                        if (await userManager.IsInRoleAsync(user, "Organization"))
                        {
                            context.Response.Redirect("/Organization");
                            return;
                        }
                        else if (await userManager.IsInRoleAsync(user, "Admin"))
                        {
                            context.Response.Redirect("/Admin");
                            return;
                        }
                        else if (await userManager.IsInRoleAsync(user, "User"))
                        {
                            context.Response.Redirect("/User");
                            return;
                        }
                    }
                }
            }
            await next(context);
        }
    }
}