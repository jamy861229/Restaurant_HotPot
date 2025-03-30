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
                .Where(e => e.MenuIsAvailable) // �L�o�u��ܥi�Ϊ���涵��
                .ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index1_Mealsincluded()
        {
            var menus = await _context.Menus
                 .Where(e => e.MenuIsAvailable) // �L�o�u��ܥi�Ϊ���涵��
                 .ToListAsync();
            return View(menus);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index2_Desserts()
        {
            var menus = await _context.Menus
                .Where(e => e.MenuIsAvailable) // �L�o�u��ܥi�Ϊ���涵��
                .ToListAsync();
            return View(menus);
        }

        // ��ܩҦ����
        public async Task<IActionResult> Index3_MenuRead()
        {
            var menus = await _context.Menus.OrderByDescending(e => e.MenuId).ToListAsync();
            return View(menus);
        }

        // ��ܷs�W��� (GET)
        public IActionResult Index4_MenuCreate()
        {
            return View();
        }

        // �B�z��洣��  (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index4_MenuCreate(MenuView menu)
        {
            // ���� ModelState ���� MenuCategory ���~
            ModelState.Remove(nameof(menu.MenuCategory));

            if (ModelState.IsValid)
            {
                // �ھ����O�s���]�m���O�W��
                switch (menu.MenuCategoryId)
                {
                    case 1:
                        menu.MenuCategory = "����";
                        break;
                    case 2:
                        menu.MenuCategory = "����";
                        break;
                    case 3:
                        menu.MenuCategory = "���I";
                        break;
                }

                _context.Menus.Add(menu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index3_MenuRead));  // �s�W���������^���C��
            }

            return View(menu);
        }

        // ��ܽs����(GET)
        public async Task<IActionResult> Index5_MenuEdit(int id)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(e => e.MenuId == id); // �ھ� ID ������ƾ�
            if (menu == null)
            {
                return NotFound(); // �p�G��椣�s�b�A��^ 404
            }

            // �Ы� MenuView �ҫ��ñN�ƾڶ�R
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

            return View(model); // �ǻ��ҫ������
        }

        // �B�z��洣��(POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index5_MenuEdit(MenuView menu)
        {
            // ���� ModelState ���� MenuCategory ���~
            ModelState.Remove(nameof(menu.MenuCategory));

            if (ModelState.IsValid)
            {
                var menuInDb = await _context.Menus.FirstOrDefaultAsync(e => e.MenuId == menu.MenuId);
                if (menuInDb == null)
                {
                    return NotFound();
                }

                // �ھ����O�s���]�m���O�W��
                switch (menu.MenuCategoryId)
                {
                    case 1:
                        menuInDb.MenuCategory = "����";
                        break;
                    case 2:
                        menuInDb.MenuCategory = "����";
                        break;
                    case 3:
                        menuInDb.MenuCategory = "���I";
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
                return RedirectToAction(nameof(Index3_MenuRead));  // ��s���������^���C��
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