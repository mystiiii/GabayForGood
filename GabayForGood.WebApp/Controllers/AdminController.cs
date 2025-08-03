using AutoMapper;
using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GabayForGood.WebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IMapper mapper;
        private readonly AppDbContext context;
        public AdminController(AppDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public IActionResult LogIn()
        {
            return View();
        }
        public IActionResult RegisterOrg()
        {
            return View();
        }
        public async Task<IActionResult> Index()
        {
            var orgs = await context.Organizations.ToListAsync();
            return View(mapper.Map<List<OrgVM>>(orgs));
        }
    }
}
