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
        //POST: Reservation/Create
        //GET: Reservation/Create
        //[HttpGet]
        //public IActionResult StoreInformation()
        //{
        //    return View();
        //}
        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StoreInformation([Bind("RestaurantId,CustomerId,ReservationName,ReservationPhone,ReservationPeople,ReservationDate")] ReservationView reservation)
        {
            if (ModelState.IsValid)
            {
                // 這裡使用 Reservation (資料庫實體) 而非 ReservationView
                ReservationView reservation1 = new ReservationView
                {
                    // 其餘欄位來自表單或預設
                    RestaurantId = reservation.RestaurantId,
                    ReservationName = reservation.ReservationName,
                    ReservationPhone = reservation.ReservationPhone,
                    ReservationPeople = reservation.ReservationPeople,
                    ReservationDate = reservation.ReservationDate,

                    // 指定固定的 CustomerID
                    CustomerId = 1,

                    // 設定系統時間給 ReservationCreatedDate
                    ReservationCreatedDate = DateTime.Now
                };

                _context.Reservations.Add(reservation1);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success");
            }
            return View(reservation);
        }

    }
}
