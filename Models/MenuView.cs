using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models;

public partial class MenuView
{
    public int MenuId { get; set; }

    [Display(Name = "菜品種類")]
    public int MenuCategoryId { get; set; }

    public string MenuCategory { get; set; } = null!;

    [Display(Name = "菜品名稱")]
    public string MenuName { get; set; } = null!;

    [Display(Name = "菜品描述")]
    public string? MenuDescription { get; set; }

    [Display(Name = "菜品價格")]
    public int MenuPrice { get; set; }

    public bool MenuIsAvailable { get; set; }

    [Display(Name = "菜品是否供應")]
    [NotMapped]
    public string? MenuAvailable { get; set; }

    [Display(Name = "菜單圖片連結")]
    public string? MenuImageUrl { get; set; }

    [NotMapped]
    public IFormFile? MenuImageFile { get; set; }  // 用來接收上傳的檔案

    public virtual ICollection<OrderItemView> OrderItems { get; set; } = new List<OrderItemView>();
}
