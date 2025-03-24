using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models;

public partial class OrderView
{
    public int OrderId { get; set; }

    public int OrderCustomerId { get; set; }

    [Required(ErrorMessage = "請選擇訂餐分店")]
    [Display(Name = "訂餐分店")]
    public int OrderRestaurantId { get; set; }

    public DateTime OrderDate { get; set; }

    public int OrderTotalAmount { get; set; }

    [Required(ErrorMessage = "請選擇訂餐類型")]
    [Display(Name = "訂餐類型")]
    public string? OrderType { get; set; }

    [NotMapped]
    public string OrderMenuName { get; set; } = null!;

    [Required(ErrorMessage = "請輸入訂餐姓名")]
    [Display(Name ="訂餐姓名")]
    public string OrderName { get; set; } = null!;

    [Required(ErrorMessage = "請輸入訂餐電話")]
    [Display(Name = "訂餐電話")]
    public string OrderPhone { get; set; } = null!;

    [Display(Name = "訂餐地址")]
    public string OrderAddress { get; set; } = null!;

    [NotMapped]
    public int RegionAddress { get; set; }

    [Required(ErrorMessage = "請輸入訂餐地址")]
    [NotMapped]
    public string StreetAddress { get; set; } = null!;

    [NotMapped]
    public string? OrderMenuImage { get; set; } = null!;

    [NotMapped]
    public string? OrderMenuDescription { get; set; } = null!;

    [NotMapped]
    public int OrderMenuId { get; set; }

    [NotMapped]
    public int OrderMenuPrice { get; set; }

    public virtual CustomerView OrderCustomer { get; set; } = null!;

    public virtual ICollection<OrderItemView> OrderItems { get; set; } = new List<OrderItemView>();

    public virtual RestaurantInfoView OrderRestaurant { get; set; } = null!;

    [NotMapped]
    public int SelectedQuantity { get; set; }

    [NotMapped]
    public string OrderStatus { get;  set; }
}
