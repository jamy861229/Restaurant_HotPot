using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Protocol;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using Restaurant.Models;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Restaurant.Controllers
{
    public class OrderController : Controller
    {
        private readonly MyDbContext _context;

        public OrderController(MyDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult GetRegionByRestaurant(int restaurantId)
        {
            var restaurant = _context.RestaurantInfos.Find(restaurantId);
            // 從資料庫中查找符合 restaurantId 的餐廳資訊

            if (restaurant != null)
            {
                var match = Regex.Match(restaurant.RestaurantAddress, @"(.+?區)");
                // 使用正則表達式匹配地址中的「XX區」

                string region = match.Success ? match.Value : "未知區域";
                // 如果匹配成功，則回傳對應的區域名稱，否則回傳「未知區域」

                return Json(new { success = true, text = region });
            }
            return Json(new { success = false });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> GetCustomerData()
        {
            int orderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("Order_CustomerId"));
            var customer = await _context.Customers
                                         .Where(c => c.CustomerId == orderCustomerId)
                                         .Select(c => new
                                         {
                                             customerName = c.CustomerName,
                                             customerPhone = c.CustomerPhone
                                         })
                                         .FirstOrDefaultAsync();

            if (customer != null)
            {
                return Json(customer);
            }
            return Json(null);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Index_Order()
        {
            // 取得餐廳列表
            var restaurantList = await _context.RestaurantInfos.ToListAsync();

            ViewBag.OrderRestaurantId = restaurantList.Select(r => new SelectListItem
            {
                Text = r.RestaurantName,
                Value = r.RestaurantId.ToString()
            }).ToList();

            // 預設區域選單為空
            ViewBag.RegionList = new List<SelectListItem>();

            // 取得目前登入使用者的 CustomerId
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                ViewBag.OrderCustomerId = 20;
                HttpContext.Session.SetString("Order_CustomerId", "20");
                ViewBag.AllowedOrderTypes = new List<string> { "DineIn" }; // 未登入者只能內用
            }
            else
            {
                ViewBag.OrderCustomerId = int.Parse(userIdClaim.Value);
                HttpContext.Session.SetString("Order_CustomerId", userIdClaim.Value);
                ViewBag.AllowedOrderTypes = new List<string> { "TakeOut", "Delivery", "DineIn" }; // 登入者可選擇全部
            }

            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index_Order(OrderView model)
        {
            HttpContext.Session.SetString("Order_Type", model.OrderType);

            if (string.IsNullOrEmpty(Convert.ToString(model.OrderRestaurantId)) || Convert.ToString(model.OrderRestaurantId) == "0")
            {
                ModelState.AddModelError("OrderRestaurantError", "!!!訂餐分店必需填寫!!!");

                // 取得餐廳列表
                var restaurantList = await _context.RestaurantInfos.ToListAsync();

                ViewBag.OrderRestaurantId = restaurantList.Select(r => new SelectListItem
                {
                    Text = r.RestaurantName,
                    Value = r.RestaurantId.ToString()
                }).ToList();

                // 預設區域選單為空
                ViewBag.RegionList = new List<SelectListItem>();

                // 取得目前登入使用者的 CustomerId
                var userIdClaim = User.FindFirst("UserId");

                if (userIdClaim == null)
                {
                    ViewBag.OrderCustomerId = 20;
                    HttpContext.Session.SetString("Order_CustomerId", "20");
                    ViewBag.AllowedOrderTypes = new List<string> { "DineIn" }; // 未登入者只能內用
                }
                else
                {
                    ViewBag.OrderCustomerId = int.Parse(userIdClaim.Value);
                    HttpContext.Session.SetString("Order_CustomerId", userIdClaim.Value);
                    ViewBag.AllowedOrderTypes = new List<string> { "TakeOut", "Delivery", "DineIn" }; // 登入者可選擇全部
                }

                return View(model);
            }

            if (model.OrderType == "DineIn")
            {
                model.OrderCustomerId = 20;
                model.OrderName = "內用";
                model.OrderPhone = "內用";
                model.OrderAddress = "內用";
            }
            else if (model.OrderType == "TakeOut")
            {
                if (string.IsNullOrEmpty(model.OrderName) || string.IsNullOrEmpty(model.OrderPhone))
                {
                    ModelState.AddModelError("TakeoutOrderError", "!!!外帶訂單需要填寫姓名和電話!!!");

                    // 取得餐廳列表
                    var restaurantList = await _context.RestaurantInfos.ToListAsync();

                    ViewBag.OrderRestaurantId = restaurantList.Select(r => new SelectListItem
                    {
                        Text = r.RestaurantName,
                        Value = r.RestaurantId.ToString()
                    }).ToList();

                    // 預設區域選單為空
                    ViewBag.RegionList = new List<SelectListItem>();

                    // 取得目前登入使用者的 CustomerId
                    var userIdClaim = User.FindFirst("UserId");

                    if (userIdClaim == null)
                    {
                        ViewBag.OrderCustomerId = 20;
                        HttpContext.Session.SetString("Order_CustomerId", "20");
                        ViewBag.AllowedOrderTypes = new List<string> { "DineIn" }; // 未登入者只能內用
                    }
                    else
                    {
                        ViewBag.OrderCustomerId = int.Parse(userIdClaim.Value);
                        HttpContext.Session.SetString("Order_CustomerId", userIdClaim.Value);
                        ViewBag.AllowedOrderTypes = new List<string> { "TakeOut", "Delivery", "DineIn" }; // 登入者可選擇全部
                    }

                    return View(model);
                }
                model.OrderAddress = "外帶";
            }
            else if (model.OrderType == "Delivery")
            {
                var RegionAddress = GetRegionByRestaurant(model.OrderRestaurantId);

                var match = Regex.Match(RegionAddress.Value.ToString(), @"台中市(\S+?區)");

                string region = match.Success ? "台中市" + match.Groups[1].Value : "未知區域";

                model.OrderAddress = region + model.StreetAddress;

                if (string.IsNullOrEmpty(model.OrderName) || string.IsNullOrEmpty(model.OrderPhone) || string.IsNullOrEmpty(model.StreetAddress))
                {
                    ModelState.AddModelError("DeliveryOrderError", "!!!外送訂單需要填寫姓名、電話和地址!!!");

                    // 取得餐廳列表
                    var restaurantList = await _context.RestaurantInfos.ToListAsync();

                    ViewBag.OrderRestaurantId = restaurantList.Select(r => new SelectListItem
                    {
                        Text = r.RestaurantName,
                        Value = r.RestaurantId.ToString()
                    }).ToList();

                    // 預設區域選單為空
                    ViewBag.RegionList = new List<SelectListItem>();

                    // 取得目前登入使用者的 CustomerId
                    var userIdClaim = User.FindFirst("UserId");

                    if (userIdClaim == null)
                    {
                        ViewBag.OrderCustomerId = 20;
                        HttpContext.Session.SetString("Order_CustomerId", "20");
                        ViewBag.AllowedOrderTypes = new List<string> { "DineIn" }; // 未登入者只能內用
                    }
                    else
                    {
                        ViewBag.OrderCustomerId = int.Parse(userIdClaim.Value);
                        HttpContext.Session.SetString("Order_CustomerId", userIdClaim.Value);
                        ViewBag.AllowedOrderTypes = new List<string> { "TakeOut", "Delivery", "DineIn" }; // 登入者可選擇全部
                    }

                    return View(model);
                }
            }

            model.OrderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("Order_CustomerId"));

            HttpContext.Session.SetString("Order_Name", model.OrderName);
            HttpContext.Session.SetString("Order_Phone", model.OrderPhone);
            HttpContext.Session.SetString("Order_RestaurantId", model.OrderRestaurantId.ToString());
            HttpContext.Session.SetString("Order_Address", model.OrderAddress);
            return RedirectToAction("Soup_Order");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Soup_Order()
        {
            var hotpotBases = await _context.Menus
                                 .Where(m => m.MenuCategory == "火鍋" && m.MenuIsAvailable == true) // `bit` 型態直接當布林值使用
                                 .Select(m => new OrderView
                                 {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                 })
                                 .ToListAsync();

            // **從 Session 取得已選擇的資料**
            var selectedSoups = HttpContext.Session.GetString("SelectedSoups");
            if (!string.IsNullOrEmpty(selectedSoups))
            {
                var selectedItems = JsonConvert.DeserializeObject<List<OrderItemView>>(selectedSoups);

                // **將已選擇的數量設定回 hotpotBases**
                foreach (var item in selectedItems)
                {
                    var menuItem = hotpotBases.FirstOrDefault(m => m.OrderMenuId == item.OrderItemMenuId);
                    if (menuItem != null)
                    {
                        menuItem.SelectedQuantity = item.OrderItemQuantity; // 設定數量
                    }
                }
            }

            ViewBag.CurrentStep = 1;
            return View(hotpotBases);
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult Soup_Order(int[] menuIds, string[] menuNames, int[] quantities, int[] unitPrices)
        {
            var selectedItems = new List<OrderItemView>();

            for (int i = 0; i < menuNames.Length; i++)
            {
                if (quantities[i] > 0)
                {
                    selectedItems.Add(new OrderItemView
                    {
                        OrderItemMenuId = menuIds[i],
                        OrderItemMenuName = menuNames[i],
                        OrderItemQuantity = quantities[i],
                        OrderItemUnitPrice = unitPrices[i]
                    });
                }
            }

            // **將選擇的資料存入 Session**
            HttpContext.Session.SetString("SelectedSoups", JsonConvert.SerializeObject(selectedItems));

            return RedirectToAction("StapleFood_Order"); // 進入下一步
        }

        [AllowAnonymous]
        public async Task<IActionResult> StapleFood_Order()
        {
            var staplefood = await _context.Menus
                                .Where(m => m.MenuCategory == "附食" && m.MenuIsAvailable == true)
                                .Select(m => new OrderView
                                {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                })
                                .ToListAsync();

            // **從 Session 取得已選擇的資料**
            var selectedStapleFoods = HttpContext.Session.GetString("SelectedStapleFoods");
            if (!string.IsNullOrEmpty(selectedStapleFoods))
            {
                var selectedItems = JsonConvert.DeserializeObject<List<OrderItemView>>(selectedStapleFoods);

                // **將已選擇的數量設定回 staplefood**
                foreach (var item in selectedItems)
                {
                    var menuItem = staplefood.FirstOrDefault(m => m.OrderMenuId == item.OrderItemMenuId);
                    if (menuItem != null)
                    {
                        menuItem.SelectedQuantity = item.OrderItemQuantity; // 設定數量
                    }
                }
            }

            ViewBag.CurrentStep = 2;
            return View(staplefood);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult StapleFood_Order(int[] menuIds, string[] menuNames, int[] quantities, int[] unitPrices, string action)
        {
            var selectedItems = new List<OrderItemView>();

            for (int i = 0; i < menuNames.Length; i++)
            {
                if (quantities[i] > 0) // 過濾掉沒有選購的
                {
                    selectedItems.Add(new OrderItemView
                    {
                        OrderItemMenuId = menuIds[i],
                        OrderItemMenuName = menuNames[i],
                        OrderItemQuantity = quantities[i],
                        OrderItemUnitPrice = unitPrices[i]
                    });
                }
            }

            HttpContext.Session.SetString("SelectedStapleFoods", JsonConvert.SerializeObject(selectedItems));

            if (action == "Previous")
                return RedirectToAction("Soup_Order");
            else
                return RedirectToAction("Dessert_Order");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Dessert_Order()
        {
            var dessert = await _context.Menus
                                .Where(m => m.MenuCategory == "甜點" && m.MenuIsAvailable == true)
                                .Select(m => new OrderView
                                {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                })
                                .ToListAsync();

            // **從 Session 取得已選擇的資料**
            var selectedDesserts = HttpContext.Session.GetString("SelectedDesserts");
            if (!string.IsNullOrEmpty(selectedDesserts))
            {
                var selectedItems = JsonConvert.DeserializeObject<List<OrderItemView>>(selectedDesserts);

                // **將已選擇的數量設定回 dessert**
                foreach (var item in selectedItems)
                {
                    var menuItem = dessert.FirstOrDefault(m => m.OrderMenuId == item.OrderItemMenuId);
                    if (menuItem != null)
                    {
                        menuItem.SelectedQuantity = item.OrderItemQuantity; // 設定數量
                    }
                }
            }

            ViewBag.CurrentStep = 3;
            return View(dessert);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Dessert_Order(int[] menuIds, string[] menuNames, int[] quantities, int[] unitPrices, string action)
        {
            var selectedItems = new List<OrderItemView>();

            for (int i = 0; i < menuNames.Length; i++)
            {
                if (quantities[i] > 0) // 過濾掉沒有選購的
                {
                    selectedItems.Add(new OrderItemView
                    {
                        OrderItemMenuId = menuIds[i],
                        OrderItemMenuName = menuNames[i],
                        OrderItemQuantity = quantities[i],
                        OrderItemUnitPrice = unitPrices[i]
                    });
                }
            }

            HttpContext.Session.SetString("SelectedDesserts", JsonConvert.SerializeObject(selectedItems));


            if (action == "Previous")
                return RedirectToAction("StapleFood_Order");
            else
                return RedirectToAction("Sum_Order");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Sum_Order()
        {
            // 從 Session 讀取訂單資訊
            var orderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("Order_CustomerId"));
            var orderType = HttpContext.Session.GetString("Order_Type");
            var orderName = HttpContext.Session.GetString("Order_Name");
            var orderPhone = HttpContext.Session.GetString("Order_Phone");
            var orderRestaurantId = HttpContext.Session.GetString("Order_RestaurantId");
            var orderAddress = HttpContext.Session.GetString("Order_Address");

            var selectedSoups = HttpContext.Session.GetString("SelectedSoups");
            var selectedStapleFoods = HttpContext.Session.GetString("SelectedStapleFoods");
            var selectedDesserts = HttpContext.Session.GetString("SelectedDesserts");

            // 解析 JSON 字串
            var soups = string.IsNullOrEmpty(selectedSoups) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedSoups);
            var stapleFoods = string.IsNullOrEmpty(selectedStapleFoods) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedStapleFoods);
            var desserts = string.IsNullOrEmpty(selectedDesserts) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedDesserts);

            // 計算總金額
            int totalAmount = soups.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
                                  stapleFoods.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
                                  desserts.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity);

            // 建立 ViewModel
            var viewModel = new OrderSummaryView
            {
                OrderCustomerId = orderCustomerId,
                OrderType = orderType,
                OrderName = orderName,
                OrderPhone = orderPhone,
                OrderRestaurant = orderRestaurantId,
                OrderAddress = orderAddress,
                OrderItems = soups.Concat(stapleFoods).Concat(desserts).ToList(),
                TotalAmount = totalAmount
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Confirm_Order()
        {
            // 從 Session 讀取訂單資訊
            var orderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("Order_CustomerId"));
            var orderType = HttpContext.Session.GetString("Order_Type");
            var orderName = HttpContext.Session.GetString("Order_Name");
            var orderPhone = HttpContext.Session.GetString("Order_Phone");
            var orderRestaurantId = HttpContext.Session.GetString("Order_RestaurantId");
            var orderAddress = HttpContext.Session.GetString("Order_Address");

            var selectedSoups = HttpContext.Session.GetString("SelectedSoups");
            var selectedStapleFoods = HttpContext.Session.GetString("SelectedStapleFoods");
            var selectedDesserts = HttpContext.Session.GetString("SelectedDesserts");

            var soups = string.IsNullOrEmpty(selectedSoups) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedSoups);
            var stapleFoods = string.IsNullOrEmpty(selectedStapleFoods) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedStapleFoods);
            var desserts = string.IsNullOrEmpty(selectedDesserts) ? new List<OrderItemView>() : JsonConvert.DeserializeObject<List<OrderItemView>>(selectedDesserts);

            int totalAmount = soups.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
                                  stapleFoods.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
                                  desserts.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity);

            // 建立訂單記錄
            var newOrder = new OrderView
            {
                OrderCustomerId = orderCustomerId,
                OrderRestaurantId = Convert.ToInt32(orderRestaurantId),
                OrderType = orderType,
                OrderName = orderName,
                OrderPhone = orderPhone,
                OrderAddress = orderAddress,
                OrderDate = DateTime.Now,
                OrderTotalAmount = totalAmount,
                OrderStatus = "未付款"
            };

            // 儲存訂單至資料庫
            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            var orderId = await _context.Orders
                                         .Where(o => o.OrderDate == newOrder.OrderDate)
                                         .Select(o => new
                                         {
                                             OrderId = o.OrderId
                                         })
                                         .FirstOrDefaultAsync();

            // 儲存訂單項目
            var orderItems = soups.Concat(stapleFoods).Concat(desserts)
                                  .Select(item => new OrderItemView
                                  {
                                      OrderItemOrderId = Convert.ToInt32(orderId.OrderId),
                                      OrderItemMenuId = item.OrderItemMenuId,
                                      OrderItemMenuName = item.OrderItemMenuName,
                                      OrderItemQuantity = item.OrderItemQuantity,
                                      OrderItemUnitPrice = item.OrderItemUnitPrice
                                  })
                                  .ToList();

            // 如果 OrderItemView 是資料庫中的實體，應該顯式設置狀態為 Added
            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();

            // 清空 Session
            HttpContext.Session.Remove("SelectedSoups");
            HttpContext.Session.Remove("SelectedStapleFoods");
            HttpContext.Session.Remove("SelectedDesserts");

            // 重導至 PayPal 付款
            return RedirectToAction("SelectPayment", orderId);
        }

        [AllowAnonymous]
        public async Task<IActionResult> SelectPayment(int orderId)
        {
            // 從資料庫查詢訂單資訊
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(); // 如果找不到訂單，回傳 404
            }

            return View(order);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult SelectPayment(int orderId, int OrderPayment)
        {
            switch (OrderPayment)
            {
                case 0:
                    // 重導至 PayPal 付款
                    return RedirectToAction("PayWithPayPal", new { orderId = orderId });
                case 1:
                    // 重導至 PayOnSite 付款
                    return RedirectToAction("PayOnSite", new { orderId = orderId });
                default:
                    // 重導至PayPalCancel
                    return RedirectToAction("PayPalCancel", new { orderId = orderId });

            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> PayWithPayPal(int orderId)
        {
            // 從資料庫查詢訂單資訊
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound(); // 如果找不到訂單，回傳 404
            }

            // 使用 PayPal 沙盒環境，使用 PayPal 開發者帳戶的 Client ID 和 Secret
            var environment = new SandboxEnvironment("AZ9jQ9Z7TqTewgvuIHxexVcg5mBcgUPlBcnCBqr7YLz5JQAyT0jwRLSvbhFJXz4xf71CwmcqeeEfqYuV", "EBtTOQRuGxl6LFsYzlPrhFvvQNYWGKlrmfBOA1-vZ8pU3ynHf7j4LOkEWR0xfL0IAvRRDCCUkeESIleO");
            var client = new PayPalHttpClient(environment);

            // 建立 PayPal 付款請求
            var paymentRequest = new OrdersCreateRequest();
            paymentRequest.Prefer("return=representation");
            paymentRequest.RequestBody(new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE", // 設定付款意圖為 CAPTURE（即付款）
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = "TWD", // 設定貨幣為台幣
                            Value = order.OrderTotalAmount.ToString() // 設定付款金額
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    // 設定付款成功和取消後的返回網址
                    ReturnUrl = Url.Action("PayPalSuccess", "Order", new { orderId = order.OrderId }, Request.Scheme),
                    CancelUrl = Url.Action("PayPalCancel", "Order", new { orderId = order.OrderId }, Request.Scheme)
                }
            });

            // 發送請求到 PayPal 取得付款連結
            var response = await client.Execute(paymentRequest);
            var result = response.Result<Order>();

            // 取得付款核准的 URL
            var approvalLink = result.Links.FirstOrDefault(link => link.Rel.Equals("approve"))?.Href;

            if (!string.IsNullOrEmpty(approvalLink))
            {
                return Redirect(approvalLink); // 將用戶導向 PayPal 付款頁面
            }

            return BadRequest("PayPal 付款初始化失敗"); // 付款初始化失敗時返回錯誤
        }

        [AllowAnonymous]
        public async Task<IActionResult> PayOnSite(int orderId)
        {
            // 查詢訂單資訊
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order); // 顯示訂單完成頁面
        }

        [AllowAnonymous]
        public async Task<IActionResult> PayPalSuccess(int orderId, string token, string PayerID)
        {
            // 查詢訂單資訊
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // 設定 PayPal 沙盒環境
            var environment = new SandboxEnvironment("AZ9jQ9Z7TqTewgvuIHxexVcg5mBcgUPlBcnCBqr7YLz5JQAyT0jwRLSvbhFJXz4xf71CwmcqeeEfqYuV", "EBtTOQRuGxl6LFsYzlPrhFvvQNYWGKlrmfBOA1-vZ8pU3ynHf7j4LOkEWR0xfL0IAvRRDCCUkeESIleO");
            var client = new PayPalHttpClient(environment);

            // 建立訂單完成請求
            var request = new OrdersCaptureRequest(token);
            request.RequestBody(new OrderActionRequest());

            // 執行付款請求
            var response = await client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                // 更新訂單狀態為已付款
                order.OrderStatus = "已付款";
                await _context.SaveChangesAsync();
                return RedirectToAction("OrderComplete", new { orderId = order.OrderId }); // 跳轉到訂單完成頁面
            }

            return BadRequest("PayPal 付款失敗"); // 付款失敗時返回錯誤
        }

        [AllowAnonymous]
        public IActionResult PayPalCancel(int orderId)
        {
            return View(); // 取消付款後返回相應頁面
        }

        [AllowAnonymous]
        public async Task<IActionResult> OrderComplete(int orderId)
        {
            // 查詢訂單資訊
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order); // 顯示訂單完成頁面
        }

    }
}
