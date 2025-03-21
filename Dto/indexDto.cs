//using Restaurant.Dto;
using Restaurant.Models;
namespace Restaurant.Dto
{

    public class OO
    {
        public string? OrderName { get; set; }
        public string? OrderType { get; set; }// 內用外帶
        public string? OrderAddress { get; set; }

        public int OrderItemQuantity { get; set; }
        public string? OrderItemMenuName { get; set; }
        public int OrderItemUnitPrice { get; set; }
        public string? SelectedSection {  get; set; }
        public DateTime OrderDate {  get; set; }


    }
    public class RRI
    {
        public int  Customer_Id;
        public string? ReservationName { get; set; }
        public string? ReservationPhone { get; set; }
        public int? ReservationPeople { get; set; }
        public string? RestaurantName { get; set; }
        public string? RestaurantAddress { get; set; }
        public DateTime ReservationDate { get; set; }
        public string? SelectedSection { get; set; }
    }
    public enum OrderTypeEnum
    {
        reserve, // 訂位
        ordering // 訂餐
    }



    public class indexDto //藥包兩個 list
    {
        public List<RRI> RRIs {get;set;}
        public CustomerView? viewModel { get; set; }
        public List<OO> OOs {get;set;}
        

        public string OrderType { get; set; }  // 使用 Enum 限制值

        
    }
}
