﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
//using Umbraco.Core.Models.Membership;
//using Microsoft.AspNet.Identity;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Restaurant.Dto;





namespace Restaurant.Controllers
{


    public class CustomersController : Controller
    {
        //CustomerView xa = new CustomerView();   //根據類別建立物件

        private readonly ILogger<CustomersController> _logger;
        private readonly MyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();  // 哈希        

        public CustomersController(ILogger<CustomersController> logger, MyDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        #region 註冊

        [AllowAnonymous] //允許匿名
        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerName,CustomerPhone,CustomerEmail,CustomerPassword,CustomerAccount,CustomerAddress,CustomerCreatedAt")] CustomerView customer)
        {

            if (ModelState.IsValid)
            {
                // 判斷是否已經註冊過了
                bool isUserExists = _context.Customers.Any(m => m.CustomerEmail == customer.CustomerEmail);
                if (isUserExists)
                {
                    ModelState.AddModelError("CustomerEmail", "❌ 該 Email 已經被註冊過了");
                    return View(customer);  // 保留輸入的資料並返回表單
                    // return RedirectToAction(nameof(Create));  // 要告訴使用者帳好已存在
                }
                string? hashedPassword = _passwordHasher.HashPassword(null!, customer.CustomerPassword); // 加密
                Debug.WriteLine("註冊時雜湊密碼: " + hashedPassword);
                CustomerView customer1 = new CustomerView
                {
                    CustomerName = customer.CustomerName,   // 姓名 
                    CustomerPhone = customer.CustomerPhone,  // 電話
                    CustomerEmail = customer.CustomerEmail,  // email
                    CustomerAddress = customer.CustomerAddress, // 地址
                    CustomerAccount = customer.CustomerAccount, // 帳號
                    CustomerPassword = hashedPassword,       // 加密密碼

                };

                _context.Add(customer1);
                await _context.SaveChangesAsync();
                return RedirectToAction(actionName: "Member_Login");
            }
            return View(customer);
        }
        #endregion  //這是收和程式碼

        #region 登入

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Member_Login()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Member_Login(string CustomerAccount, string CustomerPassword)  // 參數問題
        {

            if (ModelState.IsValid)
            {
                CustomerView? result = (from a in _context.Customers
                                        where a.CustomerAccount == CustomerAccount
                                        select a).SingleOrDefault();
                if (result == null)
                {
                    ViewBag.noMember = "去註冊啦";      // 這裡還沒弄到頁面
                    return RedirectToAction(nameof(Create));  //若找不到傳回  (帳號不存在)
                }
                var valid = _passwordHasher.VerifyHashedPassword(null!, result.CustomerPassword, CustomerPassword.Trim());
                if (valid == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
                {

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,result.CustomerAccount),//帳號
                        new Claim("UserName",result.CustomerName),//帳號
                        //new Claim("UserStatus",result.status),//身分  (沒有身分)
                        new Claim("UserId",result.CustomerId.ToString()),//會員ID
                        //new Claim(ClaimTypes.Role,"Admin")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true, // 記住登入狀態
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20) // 設定cookie 過期時間
                    };
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    //Debug.WriteLine("資料庫密碼: " + result.CustomerPassword);
                    return RedirectToAction("Index", "Customers");


                }
                else
                {
                    ViewBag.Error = "密碼錯誤，請重新輸入！";
                    Debug.WriteLine("資料庫密碼: " + result.CustomerPassword);
                    Debug.WriteLine("密碼雜湊: " + _passwordHasher.HashPassword(null!, CustomerPassword));
                    Debug.WriteLine("資料庫密碼: " + result.CustomerPassword);
                    Debug.WriteLine("資料庫密碼 (hashed): " + result.CustomerPassword);
                    Debug.WriteLine("使用者輸入密碼 (plain): " + CustomerPassword);
                    return View("Member_Login");

                }

            }
            ViewBag.Error = "請檢查輸入欄位是否正確！";
            return View("Member_Login");
        }
        #endregion

        #region 其他動作
        #endregion
        public IActionResult SomeAction()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Member_Login");
            }

            // 正常邏輯
            return View();
        }


        #region 會員專區

        // GET: Customers
        [Authorize]
        public async Task<IActionResult> Index()   // 會員專區葉面
        {
            // 使用 User.Identity.Name 來獲取當前登入的使用者帳號
            var customerAccount = User.Identity.Name;
            if (string.IsNullOrEmpty(customerAccount))
            {
                return RedirectToAction(nameof(Login));  // 如果沒有登入，則重導向登入頁面
            }
            CustomerView? member = await _context.Customers
        .Include(m => m.Orders)
        .FirstOrDefaultAsync(m => m.CustomerAccount == customerAccount);   // 假設有登入機制



            if (member == null)
            {
                return RedirectToAction(nameof(Create));
            }

            var viewModel = new CustomerView
            {
                CustomerAccount = member.CustomerAccount,
                CustomerName = member.CustomerName,
                CustomerPhone = member.CustomerPhone,
                CustomerEmail = member.CustomerEmail,
                CustomerAddress = member.CustomerAddress
                //Orders = member.Orders.Select(o => new OrderView
                //{
                //    OrderId = o.OrderId,
                //    OrderDate = o.OrderDate,
                //    OrderTotalAmount = o.OrderTotalAmount,
                //    // Status = o.Status
                //}).ToList()
            };
            var 一個什麼 = GetOrderInfo(102, "reserve") ;
            //return View(await _context.Customers.ToListAsync());
            var Data = new indexDto {RRIs= 一個什麼 ,viewModel= viewModel };
            return View( Data);
        }

        #endregion
         
        

        public class OrderRequest
        {
            public string? OrderType { get; set; }  // 訂位或訂餐的選擇
            public int? ReservationId { get; set; }  // 訂位ID，可能為空
            public int OrderId { get; set; }
        }

        [HttpPost]
        public List<RRI> GetOrderInfo(int id, string orderType)
        {
            if (orderType == "reserve")
            {
                // 假設 model.ReservationId 是前端傳來的訂位ID
                var reservationData = (from r in _context.Reservations
                                       join ri in _context.RestaurantInfos
                                       on r.RestaurantId equals ri.RestaurantId
                                       where r.ReservationId ==  id
                                       select new RRI
                                       {
                                           ReservationName = r.ReservationName,
                                           ReservationPhone = r.ReservationPhone,
                                           ReservationPeople = r.ReservationPeople,
                                           RestaurantName = ri.RestaurantName,
                                           RestaurantAddress = ri.RestaurantAddress,
                                           ReservationDate = r.ReservationDate
                                       }).ToList();
                
                if (reservationData != null)
                {
                    // 渲染訂位資料視圖，並傳遞 reservationData
                    //Debug.WriteLine(reservationData.ReservationName);
                    return reservationData;
                }
                
            }return new List<RRI> { };
            //return Json(new { message = "沒有找到相關訂位資料" });


        }
        

        //用來將視圖渲染為字符串的輔助方法
        //public string RenderViewToString(string viewName, object model)
        //{
        //    var controllerContext = this.ControllerContext;
        //    var viewData = new ViewDataDictionary<object>(this.ViewData) { Model = model };
        //    var tempData = this.TempData;
        //    using (var sw = new StringWriter())
        //    {
        //        var viewEngineResult = _viewEngine.GetView("", viewName, false);
        //        var viewContext = new ViewContext(controllerContext, viewEngineResult.View, viewData, tempData, sw, new HtmlHelperOptions());
        //        viewEngineResult.View.RenderAsync(viewContext).Wait();
        //        return sw.GetStringBuilder().ToString();
        //    }
        //}


        #region 編輯使用者資料


        [Authorize]
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(Id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( CustomerView customer)  // 這裡id =0 ?
        {
            Debug.WriteLine($"前端傳來的 Id: {customer.CustomerId}, CustomerId: {customer.CustomerId}");
            if (customer.CustomerId != customer.CustomerId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(customer.CustomerId);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    //寫進資料庫
                    existingCustomer.CustomerName = customer.CustomerName;
                    existingCustomer.CustomerPhone = customer.CustomerPhone;
                    existingCustomer.CustomerAddress = customer.CustomerAddress;

                    await _context.SaveChangesAsync();
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("Index");
        }

        #endregion




        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }





        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }


        // 自己加的
        public async Task<IActionResult> Index_Member(int? id)
        {
            //ViewBag.CName = _context.Customers.EntityType.Name;
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        #region Member_Register 沒在用
       
        public IActionResult Member_Register()
        {
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Member_Register([Bind("Customer_Id,CustomerName,CustomerPhone,CustomerEmail,CustomerPassword,CustomerBirthDate,CustomerAccount,CustomerPoints,CustomerAddress,CustomerCreatedAt")] CustomerView customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }
        #endregion


        #region 登出
        #endregion
        [HttpGet]
       // [HttpPost]
        //[AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // 確保 Cookie 真的被刪除
            Response.Cookies.Delete(".AspNetCore.Cookies");
            return RedirectToAction("Index", "Homepage");
        }
        [HttpGet]
        public string noLogin()
        {
            return "未登入";
        }
        
    }

    
}
