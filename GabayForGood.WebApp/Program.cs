using Microsoft.EntityFrameworkCore;
using GabayForGood.DataModel;
using System;
using Microsoft.AspNetCore.Identity;
using GabayForGood.WebApp.MapConfig;
using GabayForGood.Data;
using GabayForGood.WebApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

builder.Services.AddDbContext<AppDbContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("LeiServer"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login"; 
    options.LogoutPath = "/Admin/Logout"; 
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RoleBasedDefaultRouteMiddleware>();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 401 || response.StatusCode == 403)
    {
        // Check if it's an admin route
        var path = context.HttpContext.Request.Path;
        if (path.StartsWithSegments("/Admin"))
        {
            response.Redirect("/Admin/Login");
        }
        else
        {
            response.Redirect("/User/SignIn"); 
        }
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "organization",
    pattern: "Organization/{action=Index}/{id?}",
    defaults: new { controller = "Organization" });

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "user",
    pattern: "User/{action=SignIn}/{id?}",  
    defaults: new { controller = "User" });

app.Run();