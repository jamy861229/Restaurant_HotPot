using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<RestaurantInfo> RestaurantInfo { get; set; }
    }
}
