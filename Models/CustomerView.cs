namespace Restaurant.Models;

public partial class CustomerView
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string CustomerEmail { get; set; } = null!;

    public byte[] CustomerPassword { get; set; } = null!;

    public string CustomerAccount { get; set; } = null!;

    public string CustomerAddress { get; set; } = null!;

    public DateTime? CustomerCreatedAt { get; set; }

    public virtual ICollection<OrderView> Orders { get; set; } = new List<OrderView>();

    public virtual ICollection<ReservationView> Reservations { get; set; } = new List<ReservationView>();
}
