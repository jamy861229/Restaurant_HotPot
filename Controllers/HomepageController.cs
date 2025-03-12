using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class HomepageController : Controller
    {
        private readonly MyDbContext _context;
        public HomepageController(MyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous] //允許匿名
        public async Task<IActionResult> Index()
        {
            ViewBag.Demo = await _context.Carousels.ToListAsync();
            return View();
        }
    }
}
