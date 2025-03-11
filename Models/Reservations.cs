using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [Column("Reservation_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }

        
        [Column("Customer_Id")]
        public int? CustomerId { get; set; }

        [Required]
        [Column("Restaurant_Id")]
        public int RestaurantId { get; set; }

        [Required]
        [Column("Reservation_Name")]
        [StringLength(50)]
        public string ReservationName { get; set; }

        [Required]
        [Column("Reservation_Phone")]
        [StringLength(20)]
        public string ReservationPhone { get; set; }

        [Required]
        [Column("Reservation_People")]
        public int ReservationPeople { get; set; }

        [Required]
        [Column("Reservation_Date")]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Column("Reservation_CreatedDate")]
        public DateTime ReservationCreatedDate { get; set; }
    }
}
