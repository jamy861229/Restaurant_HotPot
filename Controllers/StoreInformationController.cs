using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class StoreInformationController : Controller
    {
        private readonly MyDbContext _context;

        public StoreInformationController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> StoreInformation()
        {
            var stores = await _context.RestaurantInfos.ToListAsync();
            return View(stores);
        }
        //public IActionResult StoreInformation()
        //{
        //    return View();
        //}
    }
}
