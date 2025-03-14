using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using System.Security.Claims;

namespace Restaurant.Controllers
{
    public class StoreInformationController : Controller
    {
        private readonly MyDbContext _context;

        public StoreInformationController(MyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> StoreInformation()
        {
            var stores = await _context.RestaurantInfos.ToListAsync();
            var viewModel = new ReservationRestaurantViewModel
            {
                RestaurantInfos = stores,
                Reservation = new ReservationView()  // 空的預約資料，供表單使用
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreInformation(ReservationRestaurantViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // 取得目前登入使用者的 CustomerId
                // 請確認你在登入時有將 CustomerId 放入 Claim("UserId")
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    // 如果找不到，可能尚未登入，可導向登入頁面
                    return RedirectToAction("Member_Login", "Customers");
                }
                int customerId = int.Parse(userIdClaim.Value);

                // 建立預約資料，注意這邊我們使用 ReservationView 作為實體型別（請確認資料庫對應的實體）
                var newReservation = new ReservationView
                {
                    RestaurantId = viewModel.Reservation.RestaurantId,
                    ReservationName = viewModel.Reservation.ReservationName,
                    ReservationPhone = viewModel.Reservation.ReservationPhone,
                    ReservationPeople = viewModel.Reservation.ReservationPeople,
                    ReservationDate = viewModel.Reservation.ReservationDate,
                    CustomerId = customerId, // 從登入使用者取得 CustomerId
                    ReservationCreatedDate = DateTime.Now
                };

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "預約成功！";
                return RedirectToAction("StoreInformation");
            }
            else
            {
                // Log 錯誤訊息以便排查
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var err in errors)
                {
                    Console.WriteLine(err.ErrorMessage);
                }
            }
            // 驗證失敗時重新取得分店資訊
            viewModel.RestaurantInfos = await _context.RestaurantInfos.ToListAsync();
            return View(viewModel);
        }

        // 預約成功頁面 (選用)
        public IActionResult Success()
        {
            return View();
        }

        // API：根據分店與日期查詢各小時預約人數，回傳客滿的小時清單
        [HttpGet]
        public async Task<IActionResult> GetFullyBookedHours(int restaurantId, string date)
        {
            DateTime selectedDate = DateTime.Parse(date);
            // 查詢該分店該日期所有預約資料
            var reservations = await _context.Reservations
                                .Where(r => r.RestaurantId == restaurantId && r.ReservationDate.Date == selectedDate.Date)
                                .ToListAsync();

            // 依小時分組，計算每個小時總預約人數
            var hourCapacity = reservations
                .GroupBy(r => r.ReservationDate.Hour)
                .Select(g => new { Hour = g.Key, TotalPeople = g.Sum(r => r.ReservationPeople) })
                .ToList();

            // 取得該分店的最大收客數（假設 RestaurantInfo 有 RestaurantCapacity 屬性）
            int maxCapacity = GetMaxCapacity(restaurantId);

            // 篩選總人數大於或等於最大收客數的時段
            var fullyBookedHours = hourCapacity
                                    .Where(h => h.TotalPeople >= maxCapacity)
                                    .Select(h => h.Hour)
                                    .ToList();

            return Json(fullyBookedHours);
        }

        private int GetMaxCapacity(int restaurantId)
        {
            var restaurant = _context.RestaurantInfos.FirstOrDefault(r => r.RestaurantId == restaurantId);
            return restaurant != null ? restaurant.RestaurantCapacity : 0;
        }
    }
}
