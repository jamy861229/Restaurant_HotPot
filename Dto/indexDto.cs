using Restaurant.Dto;
using Restaurant.Models;
namespace Restaurant.Dto
{
    public class RRI
    {
        public string? ReservationName { get; set; }
        public string? ReservationPhone { get; set; }
        public int? ReservationPeople { get; set; }
        public string? RestaurantName { get; set; }
        public string? RestaurantAddress { get; set; }
        public DateTime ReservationDate { get; set; }
    }


    public class indexDto //藥包兩個 list
    {
        public List<RRI>? RRIs {get;set;}
        public CustomerView viewModel { get; set; }

    }
}
