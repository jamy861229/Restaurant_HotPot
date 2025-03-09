using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class CustomerFeedbackController : Controller
    {
        private readonly MyDbContext _context;

        public CustomerFeedbackController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create_CustomerFeedback()
        {
            ViewBag.Feedback = await _context.CustomerFeedbacks.ToListAsync();

            // 取得餐廳列表
            var restaurantList = await _context.RestaurantInfos.ToListAsync();
            ViewBag.Feedback_DiningLocationId = restaurantList.Select(r => new SelectListItem
            {
                Text = r.RestaurantName,
                Value = r.RestaurantId.ToString()
            }).ToList();

            // 取得菜單列表
            var menuList = await _context.Menus.ToListAsync();
            ViewBag.Feedback_MenuId = menuList.Select(m => new SelectListItem
            {
                Text = m.MenuName,
                Value = m.MenuId.ToString()
            }).ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create_CustomerFeedback(CustomerFeedbackView model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("模型驗證錯誤：" + modelError.ErrorMessage);
                }

                // 重新載入選單
                var restaurantList = await _context.RestaurantInfos.ToListAsync();
                ViewBag.Feedback_DiningLocationId = restaurantList.Select(r => new SelectListItem
                {
                    Text = r.RestaurantName,
                    Value = r.RestaurantId.ToString()
                }).ToList();

                var menuList = await _context.Menus.ToListAsync();
                ViewBag.Feedback_MenuId = menuList.Select(m => new SelectListItem
                {
                    Text = m.MenuName,
                    Value = m.MenuId.ToString()
                }).ToList();

                return View(model);
            }


            var feedback = new CustomerFeedbackView
            {
                FeedbackName = model.FeedbackName!,
                FeedbackGender = model.FeedbackGender!,
                FeedbackDateTime = model.FeedbackDateTime ?? DateTime.Now,
                FeedbackDiningLocationId = model.FeedbackDiningLocationId,
                FeedbackMenuId = model.FeedbackMenuId ?? 0,
                FeedbackPhone = model.FeedbackPhone!,
                FeedbackEmail = model.FeedbackEmail!,
                FeedbackContent = model.FeedbackContent!,
                FeedbackTime = DateTime.Now,
            };

            // 取得餐廳名稱
            var restaurant = await _context.RestaurantInfos.FindAsync(model.FeedbackDiningLocationId);
            if (restaurant != null)
            {
                feedback.FeedbackDiningLocation = restaurant.RestaurantName;
            }

            // 取得菜單名稱
            var menu = await _context.Menus.FindAsync(model.FeedbackMenuId);
            if (menu != null)
            {
                feedback.FeedbackMenuName = menu.MenuName;
            }

            _context.CustomerFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Create_CustomerFeedback");
        }
    }
}

