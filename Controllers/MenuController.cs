using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class MenuController : Controller
    {
        private readonly ILogger<MenuController> _logger;
        private readonly MyDbContext _context;

        public MenuController(ILogger<MenuController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index_Musteat()
        {
            var menus = await _context.Menus.ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index1_Mealsincluded()
        {
            var menus = await _context.Menus.ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index2_Desserts()
        {
            var menus = await _context.Menus.ToListAsync();
            return View(menus);
        }

        //public async Task<IActionResult> Index3_Menuadmin()
        //{
        //    var menus = await _context.Menus.ToListAsync();
        //    return View(menus);
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
