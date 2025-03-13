using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Restaurant.Models
{
    public class ReservationRestaurantViewModel
    {
        [BindNever]
        [ValidateNever]
        // 用於顯示「分店資訊列表」
        public IEnumerable<Restaurant.Models.RestaurantInfoView> RestaurantInfos { get; set; }

        // 用於接收「訂位表單」的資料
        public ReservationView Reservation { get; set; }
    }
}
