using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class LocationController : Controller
    {
        private readonly MyDbContext _context;

        public LocationController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Location()
        {
            var stores = await _context.RestaurantInfo.ToListAsync();
            return View(stores);
        }
    }
}
