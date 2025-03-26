using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models;

public partial class MenuView
{
    public int MenuId { get; set; }

    public int MenuCategoryId { get; set; }

    public string MenuCategory { get; set; } = null!;

    public string MenuName { get; set; } = null!;

    public string? MenuDescription { get; set; }

    public int MenuPrice { get; set; }

    public bool MenuIsAvailable { get; set; }

    [NotMapped]
    public string? MenuAvailable { get; set; }

    public string? MenuImageUrl { get; set; }

    public virtual ICollection<OrderItemView> OrderItems { get; set; } = new List<OrderItemView>();
}
