using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class HomepageController : Controller
    {
        private readonly HomepageDbContext _context;
        public HomepageController(HomepageDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.Demo = await _context.Carousel.ToListAsync();
            return View();
        }
    }
}
