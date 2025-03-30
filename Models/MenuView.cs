using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models;

public partial class MenuView
{
    [Display(Name = "編號")]
    public int MenuId { get; set; }

    [Display(Name = "類別")]
    public string MenuCategory { get; set; } = null!;

    [Display(Name = "名稱")]
    public string MenuName { get; set; } = null!;

    [Display(Name = "描述")]
    public string? MenuDescription { get; set; }

    [Display(Name = "價格")]
    public int MenuPrice { get; set; }

    [Display(Name = "狀態")]
    public bool MenuIsAvailable { get; set; }

    [Display(Name = "類別編號")]
    public int MenuCategoryId { get; set; }

    [NotMapped]
    public string? MenuAvailable { get; set; }

    [Display(Name = "圖片 / 路徑")]
    public string? MenuImageUrl { get; set; }

    public virtual ICollection<OrderItemView> OrderItems { get; set; } = new List<OrderItemView>();
}