using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
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
                ViewBag.OrderCustomerId = 14;
                HttpContext.Session.SetString("Order_CustomerId", "14");
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
        public IActionResult Index_Order(OrderView model)
        {
            HttpContext.Session.SetString("Order_Type", model.OrderType);

            if (model.OrderType == "DineIn")
            {
                model.OrderCustomerId = 14;
                model.OrderName = "內用";
                model.OrderPhone = "內用";
                model.OrderAddress = "內用";
            }
            else if (model.OrderType == "TakeOut")
            {
                model.OrderAddress = "外帶";
                model.OrderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("OrderCustomerId"));
            }
            else
            {
                var RegionAddress = GetRegionByRestaurant(model.OrderRestaurantId);

                var match = Regex.Match(RegionAddress.Value.ToString(), @"台中市(\S+?區)");

                string region = match.Success ? "台中市" + match.Groups[1].Value : "未知區域";

                model.OrderAddress = region + model.StreetAddress;
                model.OrderCustomerId = Convert.ToInt32(HttpContext.Session.GetString("Order_CustomerId"));
            }

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
                                .Where(m => m.MenuCategory == "火鍋")
                                .Select(m => new OrderView
                                {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                })
                                .ToListAsync();

            ViewBag.CurrentStep = 1;
            return View(hotpotBases);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Soup_Order(int[] menuIds, string[] menuNames, int[] quantities, decimal[] unitPrices)
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

            HttpContext.Session.SetString("SelectedSoups", JsonConvert.SerializeObject(selectedItems));
            return RedirectToAction("StapleFood_Order");
        }


        [AllowAnonymous]
        public async Task<IActionResult> StapleFood_Order()
        {
            var staplefood = await _context.Menus
                                .Where(m => m.MenuCategory == "附食")
                                .Select(m => new OrderView
                                {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                })
                                .ToListAsync();

            ViewBag.CurrentStep = 2;
            return View(staplefood);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult StapleFood_Order(int[] menuIds, string[] menuNames, int[] quantities, decimal[] unitPrices, string action)
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
                                .Where(m => m.MenuCategory == "甜點")
                                .Select(m => new OrderView
                                {
                                    OrderMenuId = m.MenuId,
                                    OrderMenuName = m.MenuName,
                                    OrderMenuPrice = m.MenuPrice,
                                    OrderMenuImage = m.MenuImageUrl,
                                    OrderMenuDescription = m.MenuDescription
                                })
                                .ToListAsync();

            ViewBag.CurrentStep = 3;
            return View(dessert);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Dessert_Order(int[] menuIds, string[] menuNames, int[] quantities, decimal[] unitPrices, string action)
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
            decimal totalAmount = soups.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
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

            decimal totalAmount = soups.Sum(x => x.OrderItemUnitPrice * x.OrderItemQuantity) +
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
                TotalAmount = totalAmount
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

            return RedirectToAction("Index_Order");
        }

    }
}
