using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        // 連結到 Customer
        public int CustomerId { get; set; }

        [Required]
        public string Token { get; set; }

        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CustomerId")]
        public virtual CustomerView Customers { get; set; }
    }
}
