using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public enum OrderStatus { Pending, Confirmed, Completed, Cancelled }
    public enum PaymentStatus { Unpaid, Paid, Refunded }
    public enum PaymentMethod { Cash, CreditCard, OnlineBanking, EWallet }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Order Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [MaxLength(300)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; } = string.Empty;

        // Navigation
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ShowtimeId { get; set; }

        [MaxLength(200)]
        public string MovieTitle { get; set; } = string.Empty;

        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // Navigation
        public virtual Order? Order { get; set; }
        public virtual Showtime? Showtime { get; set; }

        public decimal SubTotal => UnitPrice * Quantity;
    }
}
