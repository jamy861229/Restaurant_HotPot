namespace Restaurant.Models
{
    public class CompositeReservationViewModel
    {
        public IEnumerable<ReservationView> Reservations { get; set; }
        public IEnumerable<RestaurantInfoView> RestaurantInfos { get; set; }
    }

}
