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

        public decimal UnitPrice { get; set; }

        // Navigation
        public virtual Cart? Cart { get; set; }
        public virtual Showtime? Showtime { get; set; }

        public decimal SubTotal => UnitPrice * Quantity;
    }
}
