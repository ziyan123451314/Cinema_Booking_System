using System.ComponentModel.DataAnnotations.Schema; // 👈 新增：引入 Schema 命名空间

namespace CinemaBookingSystem.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal Total => CartItems.Sum(i => i.UnitPrice * i.Quantity);
        public int ItemCount => CartItems.Sum(i => i.Quantity);
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ShowtimeId { get; set; }
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18, 2)")] // 👈 新增：告诉数据库这个金额最多18位数，保留2位小数
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Comma-separated seat labels chosen on the seat map, e.g. "A1,A2,B5".
        /// Empty string means no specific seats were assigned (legacy/fallback).
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(500)]
        public string SeatLabels { get; set; } = string.Empty;

        // Navigation
        public virtual Cart? Cart { get; set; }
        public virtual Showtime? Showtime { get; set; }

        public decimal SubTotal => UnitPrice * Quantity;
    }
}