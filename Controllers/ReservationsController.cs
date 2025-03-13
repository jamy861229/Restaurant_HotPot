using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class ReservationController : Controller
    {
        private readonly MyDbContext _context;

        public ReservationController(MyDbContext context)
        {
            _context = context;
        }

        // GET: Reservation/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationView model)
        {
            if (ModelState.IsValid)
            {
                // 根據表單中門市名稱(例如 BranchName)查詢對應的餐廳資料
                var restaurant = await _context.RestaurantInfos
                    .FirstOrDefaultAsync(r => r.RestaurantName == model.RestaurantName);

                if (restaurant == null)
                {
                    ModelState.AddModelError("", "查無此餐廳資料！");
                    return View(model);
                }

                // 建立新的 Reservation 實體，僅設定餐廳ID與其他訂位資料
                var reservation = new ReservationView
                {
                    RestaurantId = restaurant.RestaurantId, // 從查詢結果取得對應的 ID
                    ReservationName = model.ReservationName,
                    ReservationPhone = model.ReservationPhone,
                    ReservationPeople = model.ReservationPeople,
                    ReservationDate = model.ReservationDate
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success");
            }
            return View(model);
        }

        // 訂位成功頁面
        public IActionResult Success()
        {
            return View();
        }
    }
}
