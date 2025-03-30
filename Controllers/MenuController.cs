using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace Restaurant.Controllers
{
    public class MenuController : Controller
    {
        private readonly MyDbContext _context;

        public MenuController(MyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index_Musteat()
        {
            var menus = await _context.Menus
                .Where(e => e.MenuIsAvailable) // 過濾只顯示可用的菜單項目
                .ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index1_Mealsincluded()
        {
            var menus = await _context.Menus
                 .Where(e => e.MenuIsAvailable) // 過濾只顯示可用的菜單項目
                 .ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index2_Desserts()
        {
            var menus = await _context.Menus
                .Where(e => e.MenuIsAvailable) // 過濾只顯示可用的菜單項目
                .ToListAsync();
            return View(menus);
        }

        // 顯示所有菜單
        public async Task<IActionResult> Index3_MenuRead()
        {
            var menus = await _context.Menus.OrderByDescending(e => e.MenuId).ToListAsync();
            return View(menus);
        }

        // 顯示新增表單 (GET)
        public IActionResult Index4_MenuCreate()
        {
            return View();
        }

        // 處理表單提交  (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index4_MenuCreate(MenuView menu)
        {
            // 移除 ModelState 中的 MenuCategory 錯誤
            ModelState.Remove(nameof(menu.MenuCategory));

            if (ModelState.IsValid)
            {
                // 根據類別編號設置類別名稱
                switch (menu.MenuCategoryId)
                {
                    case 1:
                        menu.MenuCategory = "火鍋";
                        break;
                    case 2:
                        menu.MenuCategory = "附食";
                        break;
                    case 3:
                        menu.MenuCategory = "甜點";
                        break;
                }

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index3_MenuRead));  // 新增完成後跳轉回菜單列表
            }

            return View(menu);
        }

        // 顯示編輯表單(GET)
        public async Task<IActionResult> Index5_MenuEdit(int id)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(e => e.MenuId == id); // 根據 ID 獲取菜單數據
            if (menu == null)
            {
                return NotFound(); // 如果菜單不存在，返回 404
            }

            // 創建 MenuView 模型並將數據填充
            var model = new MenuView
            {
                MenuId = menu.MenuId,
                MenuCategory = menu.MenuCategory,
                MenuCategoryId = menu.MenuCategoryId,
                MenuName = menu.MenuName,
                MenuDescription = menu.MenuDescription,
                MenuPrice = menu.MenuPrice,
                MenuIsAvailable = menu.MenuIsAvailable,
                MenuImageUrl = menu.MenuImageUrl
            };

            return View(model); // 傳遞模型到視圖
        }

        // 處理表單提交(POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index5_MenuEdit(MenuView menu)
        {
            // 移除 ModelState 中的 MenuCategory 錯誤
            ModelState.Remove(nameof(menu.MenuCategory));

            if (ModelState.IsValid)
            {
                var menuInDb = await _context.Menus.FirstOrDefaultAsync(e => e.MenuId == menu.MenuId);
                if (menuInDb == null)
                {
                    return NotFound();
                }

                // 根據類別編號設置類別名稱
                switch (menu.MenuCategoryId)
                {
                    case 1:
                        menuInDb.MenuCategory = "火鍋";
                        break;
                    case 2:
                        menuInDb.MenuCategory = "附食";
                        break;
                    case 3:
                        menuInDb.MenuCategory = "甜點";
                        break;
                }
                menuInDb.MenuCategoryId = menu.MenuCategoryId;
                menuInDb.MenuName = menu.MenuName;
                menuInDb.MenuDescription = menu.MenuDescription;
                menuInDb.MenuPrice = menu.MenuPrice;
                menuInDb.MenuIsAvailable = menu.MenuIsAvailable;
                menuInDb.MenuImageUrl = menu.MenuImageUrl;

                _context.Menus.Update(menuInDb);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index3_MenuRead));  // 更新完成後跳轉回菜單列表
            }

            return View(menu);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}