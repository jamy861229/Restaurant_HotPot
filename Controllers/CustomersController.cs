using System;
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
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Restaurant.Services;





namespace Restaurant.Controllers
{


    public class CustomersController : Controller
    {
        //CustomerView xa = new CustomerView();   //根據類別建立物件

        private readonly ILogger<CustomersController> _logger;
        private readonly MyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailService _mailService; // 新增這行
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();  // 哈希        

        public CustomersController(
          ILogger<CustomersController> logger,
          MyDbContext context,
          IHttpContextAccessor httpContextAccessor,
          EmailService mailService  // <-- 從 DI 注入 EmailService
      )
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService; // <-- 將注入的實例指定給欄位
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
            //重導向會員專區
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Customers");
            }
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


                    await HttpContext.SignInAsync("MyCookie",
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


        public class OrderRequest
        {
            public string? OrderType { get; set; }  // 訂位或訂餐的選擇
            public int? ReservationId { get; set; }  // 訂位ID，可能為空
            public int OrderId { get; set; }
        }


        #region 會員專區

        // GET: Customers
        [Authorize(AuthenticationSchemes = "MyCookie")]
        public async Task<IActionResult> Index(indexDto indexDto)   // 會員專區葉面 ordertype
        {
            // 使用 User.Identity.Name 來獲取當前登入的使用者帳號
            var customerAccount = User.Identity.Name;
            var userId = User.FindFirst("UserId")?.Value; // **透過 Claims 取得 UserId**   這是 customerId
            
            if (string.IsNullOrEmpty(customerAccount) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));  // 如果沒有登入，則重導向登入頁面
            }
           
            // 讀取cookie 

            CustomerView? member = await _context.Customers
                //.Include(m => m.Orders)
                .FirstOrDefaultAsync(m => m.CustomerAccount == customerAccount);   // 假設有登入機制
            if (member != null)
            {
               
                
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

                //indexDto indexDto = new indexDto { } ;
                //從前端來判斷使用者選擇甚麼 (reserve  Ordering) 在看要執行什麼方法
                if (indexDto.OrderType == "ordering")
                {
                    var 兩個甚麼 = GetOrderItemsInfo(userId, indexDto.OrderType);    // 這裡 id 是寫死了
                    var Data = new indexDto { RRIs = new List<RRI>(), OOs = 兩個甚麼, viewModel = viewModel, OrderType = "ordering" };
                    return View(Data);
                }
                else if (indexDto.OrderType == "reserve")
                {
                    var 一個什麼 = GetOrderInfo(userId, indexDto.OrderType);  //這裡的id 查的是 訂位id
                    var Data = new indexDto { RRIs = 一個什麼, viewModel = viewModel, OOs = new List<OO>(), OrderType = "reserve" };
                    return View(Data);
                }
                else
                {
                    var 一個什麼 = GetOrderInfo(userId, indexDto.OrderType);  //這裡的id 查的是 訂位id
                    var Data = new indexDto { RRIs = 一個什麼, viewModel = viewModel, OOs = new List<OO>() };
                    return View(Data);
                }

                //else
                //{
                //    return View(new indexDto {viewModel=viewModel,RRIs = new List<RRI> (),OOs=new List<OO> () } );   //為何要定義indexDto 因為有兩種資料來自不同的查詢 為了要顯示在前端必須把它們合併成 一個 class 
                //}


                    //var 一個什麼 = GetOrderInfo(userId, "reserve");  //這裡的id 查的是 訂位id
                    //var 兩個甚麼 = GetOrderItemsInfo(13, "Ordering");
                    //return View(await _context.Customers.ToListAsync());
                    //var Data = new indexDto { RRIs = 一個什麼, viewModel = viewModel,OOs = 兩個甚麼 };
                    //return View(Data);
                }
            return RedirectToAction(nameof(Create));
        }

        


        #endregion



        
        // 呼叫訂位資料的方法
        [HttpPost]
        public List<RRI> GetOrderInfo(string id, string orderType)  //這裡的id 要改用 CustomerId 來查找
        {
            if (orderType == "reserve")
            {
                // 假設 model.ReservationId 是前端傳來的訂位ID
                var reservationData = (from r in _context.Reservations
                                       join ri in _context.RestaurantInfos
                                       on r.RestaurantId equals ri.RestaurantId
                                       join C in _context.Customers
                                       on r.CustomerId equals C.CustomerId
                                       where r.CustomerId.ToString() == id
                                       select new RRI
                                       {
                                           Customer_Id = r.CustomerId,
                                           ReservationName = r.ReservationName,
                                           ReservationPhone = r.ReservationPhone,
                                           ReservationPeople = r.ReservationPeople,
                                           RestaurantName = ri.RestaurantName,
                                           RestaurantAddress = ri.RestaurantAddress,
                                           ReservationDate = r.ReservationDate,
                                           SelectedSection = "reserve"  // 預設顯示訂位資料
                                       }).ToList();

                if (reservationData != null)
                {
                    // 渲染訂位資料視圖，並傳遞 reservationData
                    //Debug.WriteLine(reservationData.ReservationName);
                    return reservationData;
                }


            }
            return new List<RRI> { };
            //return Json(new { message = "沒有找到相關訂位資料" });
        }

        

        [HttpPost]
        public List<OO> GetOrderItemsInfo(string id, string orderType)  //這裡的id 要改用 CustomerId 來查找
        {
            if (orderType == "ordering") {
                // 假設 model.ReservationId 是前端傳來的訂位ID
                var orderingData = (from o in _context.Orders
                                    join oi in _context.OrderItems
                                    on o.OrderId equals oi.OrderItemOrderId
                                    join  C  in _context.Customers
                                    on o.OrderCustomerId equals C.CustomerId
                                    where C.CustomerId.ToString() == id
                                    select new OO
                                    {                                        
                                        OrderName = o.OrderName,
                                        OrderType = o.OrderType,
                                        OrderAddress = o.OrderAddress,
                                        OrderItemQuantity = oi.OrderItemQuantity,
                                        OrderItemMenuName = oi.OrderItemMenuName,
                                        OrderItemUnitPrice = oi.OrderItemUnitPrice,
                                        OrderDate=o.OrderDate ,
                                        SelectedSection = "ordering"
                                    }).ToList();
            
            if (orderingData != null)
            {
                // 渲染訂位資料視圖，並傳遞 reservationData
                //Debug.WriteLine(reservationData.ReservationName);
                return orderingData;
            }

        }
            return new List<OO> { };
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

        [HttpGet]
        [Authorize(AuthenticationSchemes = "MyCookie")]
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? Id)
        {
            var userId = User.FindFirst("UserId")?.Value; // **透過 Claims 取得 UserId**   這是 customerId
            if (userId == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(Convert.ToInt32(userId));
            if (customer == null)
            {
                return View("Create");
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(AuthenticationSchemes = "MyCookie")]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerView customer)  // 這裡id =0 ?
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



        #region 用不到的程式碼
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
        #endregion

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
        [HttpPost]
        // [HttpPost]
        //[AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookie");
            // 確保 Cookie 真的被刪除
            // Response.Cookies.Delete(".AspNetCore.Cookies");  //手動
            return RedirectToAction("Index", "Homepage");
        }
        [HttpGet]
        public string noLogin()
        {
            return "未登入";
        }

        #region 忘記密碼 / 重設密碼

        // GET: /Customers/ForgotPassword
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Customers/ForgotPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Message = "請輸入有效的 Email。";
                return View();
            }

            // 根據輸入的 Email 找出對應的客戶
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerEmail == email);
            // 為防資料探測，無論找不到或找到，都回傳相同訊息
            ViewBag.Message = "若此信箱存在，系統已寄出重設密碼的連結。";

            if (customer == null)
            {
                return View();
            }

            // 產生 token (以 GUID 為例)
            string token = Guid.NewGuid().ToString();

            // 建立 PasswordResetToken 紀錄
            var resetToken = new PasswordResetToken
            {
                CustomerId = customer.CustomerId,
                Token = token,
                ExpiryTime = DateTime.UtcNow.AddHours(1), // token 有效 1 小時
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // 產生重設密碼連結 (請根據你路由設定調整)
            var resetLink = Url.Action("ResetPassword", "Customers", new { token = token }, Request.Scheme);
            var subject = "重設密碼通知";
            var body = $"請點擊以下連結重設密碼：{resetLink}";

            try
            {
                await _mailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation("寄信成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寄信失敗");
                ViewBag.Error = "寄送信件時發生錯誤：" + ex.Message;
            }

            return View();
        }
        // GET: /Customers/ResetPassword?token=xxxx
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Member_Login");
            }

            var resetToken = await _context.PasswordResetTokens
                .Include(rt => rt.Customers) // 注意: 若實體是 CustomerView，請改成 rt.Customer
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsUsed);

            if (resetToken == null || resetToken.ExpiryTime < DateTime.UtcNow)
            {
                ViewBag.Error = "Token 無效或已過期，請重新申請。";
                return View("ForgotPassword");
            }

            // 傳遞 token 至 View（以 hidden field 帶入 POST）
            ViewBag.Token = token;
            return View();
        }

        // POST: /Customers/ResetPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                ViewBag.Error = "參數錯誤。";
                return View();
            }

            var resetToken = await _context.PasswordResetTokens
                .Include(rt => rt.Customers) // 注意: 若實體是 CustomerView，請改成 rt.Customer
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsUsed);

            if (resetToken == null || resetToken.ExpiryTime < DateTime.UtcNow)
            {
                ViewBag.Error = "Token 無效或已過期，請重新申請。";
                return View("ForgotPassword");
            }

            // 更新客戶密碼 (這裡進行哈希處理)
            var customer = resetToken.Customers;
            customer.CustomerPassword = _passwordHasher.HashPassword(null, newPassword);

            // 標記 token 已使用
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();

            TempData["Message"] = "密碼重設成功，請使用新密碼登入。";
            return RedirectToAction("Member_Login");
        }

        #endregion

    }


}
