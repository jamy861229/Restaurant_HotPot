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
        // POST: StoreInformation (處理預約表單提交)
        public async Task<IActionResult> StoreInformation(ReservationRestaurantViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // 這裡假設實際存取 Reservations 資料表的實體為 Reservation
                // 如果您的 DbSet 型別為 ReservationView，也可以直接使用 ReservationView
                var newReservation = new ReservationView
                {
                    RestaurantId = viewModel.Reservation.RestaurantId,
                    ReservationName = viewModel.Reservation.ReservationName,
                    ReservationPhone = viewModel.Reservation.ReservationPhone,
                    ReservationPeople = viewModel.Reservation.ReservationPeople,
                    ReservationDate = viewModel.Reservation.ReservationDate,
                    CustomerId = 1,
                    ReservationCreatedDate = DateTime.Now
                };

                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "預約成功！";
                return RedirectToAction("StoreInformation");
            }
            else
            {
                // 這裡可以用來看是哪個欄位出錯
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var err in errors)
                {
                    Console.WriteLine(err.ErrorMessage);
                }
            }
            // 驗證失敗時，重新取得門市列表資料，再回傳 ViewModel
            viewModel.RestaurantInfos = await _context.RestaurantInfos.ToListAsync();
            return View(viewModel);
        }

        // 預約成功頁面 (選用)
        public IActionResult Success()
        {
            return View();
        }
    }

}

