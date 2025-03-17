using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models;

public partial class OrderItemView
{
    public int OrderItemId { get; set; }

    public int OrderItemOrderId { get; set; }

    public int OrderItemMenuId { get; set; }

    public int OrderItemQuantity { get; set; }

    public string? OrderItemMenuName { get; set; }

    public decimal OrderItemUnitPrice { get; set; }

    public virtual MenuView OrderItemMenu { get; set; } = null!;

    public virtual OrderView OrderItemOrder { get; set; } = null!;

}

public partial class OrderSummaryView
{
    [Display(Name = "訂餐類型")]
    public string? OrderType { get; set; }

    [Display(Name = "訂餐姓名")]
    public string OrderName { get; set; } = null!;

    [Display(Name = "訂餐電話")]
    public string OrderPhone { get; set; } = null!;

    [Display(Name = "訂餐地址")]
    public string OrderAddress { get; set; } = null!;

    public string OrderRestaurant { get; set; }

    [NotMapped]
    public decimal TotalAmount { get; set; }

    [NotMapped]
    public List<OrderItemView> OrderItems { get; set; }

}
