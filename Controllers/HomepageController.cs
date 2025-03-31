using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class HomepageController : Controller
    {
        private readonly MyDbContext _context;
        public HomepageController(MyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous] //允許匿名
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.Demo = (await _context.Carousels.ToListAsync()).OrderBy(x => x.CarouselDisplayOrder).ToList();
            }
            catch (SqlException ex)
            {
                ViewBag.ErrorMessage = "請重新確認資料庫的網路設定。";
                Console.WriteLine(ex.Message);
            }
            return View();
        }
    }
}
