using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using System.Security.Claims;
using System.Text.Json;

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
        [AllowAnonymous]
        public async Task<IActionResult> StoreInformation()
        {
            var stores = await _context.RestaurantInfos.ToListAsync();

            // 先查出所有 "分店ID + 日期 + 總人數" 以及分店容量
            var storeBookedDates = await _context.Reservations
                .GroupBy(r => new { r.RestaurantId, DateOnly = r.ReservationDate.Date })
                .Select(g => new
                {
                    g.Key.RestaurantId,
                    Date = g.Key.DateOnly,
                    TotalPeople = g.Sum(r => r.ReservationPeople)
                })
                .Join(
                    _context.RestaurantInfos,
                    res => res.RestaurantId,
                    rest => rest.RestaurantId,
                    (res, rest) => new
                    {
                        res.RestaurantId,
                        res.Date,
                        res.TotalPeople,
                        Capacity = rest.RestaurantCapacity
                    }
                )
                // 找出該店當天總人數 >= 該店容量 => 該日期客滿
                .Where(g => g.TotalPeople >= g.Capacity)
                .Select(g => new { g.RestaurantId, g.Date })
                .ToListAsync();

            // 接下來，我們要把上面結果整理成 Dictionary< int, List<string> >
            // key: RestaurantId, value: 該店已客滿日期清單 (yyyy-MM-dd)
            var storeDatesDict = storeBookedDates
                .GroupBy(x => x.RestaurantId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(d => d.Date.ToString("yyyy-MM-dd")).Distinct().ToList()
                );

            // 將字典序列化成 JSON
            // 最終格式類似: { "1": ["2025-03-15","2025-03-20"], "2": ["2025-04-10"] ... }
            var storeDatesDictJson = JsonSerializer.Serialize(storeDatesDict);
            ViewBag.StoreBookedDatesDict = storeDatesDictJson;

            var viewModel = new ReservationRestaurantViewModel
            {
                RestaurantInfos = stores,
                Reservation = new ReservationView()
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
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"欄位：{entry.Key}，錯誤：{error.ErrorMessage}");
                    }
                }

                // 驗證失敗時重新取得分店資訊
                viewModel.RestaurantInfos = await _context.RestaurantInfos.ToListAsync();
                return View(viewModel);
            }
        }
    }
}
