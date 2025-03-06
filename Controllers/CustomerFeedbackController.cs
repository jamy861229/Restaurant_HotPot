using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class CustomerFeedbackController : Controller
    {
        private readonly RestaurantContext _context;

        public CustomerFeedbackController(RestaurantContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Create_CustomerFeedback()
        {
            ViewBag.Feedback = await _context.CustomerFeedback.ToListAsync();

            // 取得餐廳列表
            var restaurantList = await _context.RestaurantInfos.ToListAsync();
            ViewBag.Feedback_DiningLocationId = restaurantList.Select(r => new SelectListItem
            {
                Text = r.RestaurantName,
                Value = r.RestaurantRestaurantId.ToString()
            }).ToList();

            // 取得菜單列表
            var menuList = await _context.Menus.ToListAsync();
            ViewBag.Feedback_MenuId = menuList.Select(m => new SelectListItem
            {
                Text = m.MenuItemName,
                Value = m.MenuMenuId.ToString()
            }).ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create_CustomerFeedback
         ([Bind("Feedback_FeedbackId,Feedback_Name,Feedback_Gender," +
            "Feedback_DiningLocationId,Feedback_DiningLocation," +
            "Feedback_Phone,Feedback_Email,Feedback_Content,Feedback_Time," +
            "Feedback_DateTime,Feedback_MenuId,Feedback_MenuName")] 
        CustomerFeedbackView model)
        {
            if (ModelState.IsValid)
            {
                model.Feedback_Time = DateTime.Now;

                // 取得餐廳名稱
                var restaurant = await _context.RestaurantInfos.FindAsync(model.Feedback_DiningLocationId);
                if (restaurant != null)
                {
                    model.Feedback_DiningLocation = restaurant.RestaurantName;
                }

                // 取得菜單名稱
                var menu = await _context.Menus.FindAsync(model.Feedback_MenuId);
                if (menu != null)
                {
                    model.Feedback_MenuName = menu.MenuItemName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Create_CustomerFeedback");
            }

            ViewBag.Feedback_DiningLocationId = (await _context.RestaurantInfos.ToListAsync()).Select(r => new SelectListItem
            {
                Text = r.RestaurantName,
                Value = r.RestaurantRestaurantId.ToString()
            }).ToList();

            ViewBag.Feedback_MenuId = (await _context.Menus.ToListAsync()).Select(m => new SelectListItem
            {
                Text = m.MenuItemName,
                Value = m.MenuMenuId.ToString()
            }).ToList();

            return View(model);
        }


    }
}

