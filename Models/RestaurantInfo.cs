using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    [Table("RestaurantInfo")]  // 假設資料表名稱為 RestaurantInfo
    public class RestaurantInfo
    {
        [Key]
        [Column("Restaurant_Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RestaurantId { get; set; }

        [Required]
        [Column("Restaurant_Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("Restaurant_Address")]
        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        [Column("Restaurant_Phone")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [Column("Restaurant_ImageUrl")]
        [StringLength(255)]
        public string ImageUrl { get; set; }

        [Required]
        [Column("Restaurant_OpeningHours")]
        [StringLength(255)]
        public string OpeningHours { get; set; }

        [Required]
        [Column("Restaurant_CreatedAt", TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; }

        [Column("Restaurant_LastOrderTime")]
        [StringLength(50)]
        public string LastOrderTime { get; set; }

        [Column("Restaurant_MapEmbedUrl")]
        [StringLength(500)]
        public string MapEmbedUrl { get; set; }

        [Column("Restaurant_InfoPosition")]
        [StringLength(10)]
        public string InfoPosition { get; set; }

        [Required]
        [Column("Restaurant_Capacity")]
        public int Capacity { get; set; }

        [Required]
        [Column("Restaurant_ReservationCount")]
        public int ReservationCount { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
