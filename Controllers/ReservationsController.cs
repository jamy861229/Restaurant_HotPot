using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using System;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly MyDbContext _context;

        public ReservationsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: /Reservations
        [HttpGet]
        public IActionResult Index()
        {
            // 假設你想要顯示一批門市資訊
            var stores = _context.RestaurantInfo.ToList();
            return View(stores);
        }

        // POST: /Reservations
        [HttpPost]
        public async Task<IActionResult> Index(Reservation model)
        {
            if (ModelState.IsValid)
            {
                model.ReservationCreatedDate = DateTime.Now;
                _context.Reservation.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "預約成功！";
                return RedirectToAction(nameof(Index));
            }
            // 驗證失敗就重新載入同一頁
            var stores = _context.RestaurantInfo.ToList();
            return View(stores);
        }
    }
}
