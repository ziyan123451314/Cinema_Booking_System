using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Showtime
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        [Display(Name = "Show Date")]
        [DataType(DataType.Date)]
        public DateTime ShowDate { get; set; }

        [Display(Name = "Show Time")]
        [DataType(DataType.Time)]
        public TimeSpan ShowTime { get; set; }

        [MaxLength(50)]
        public string Hall { get; set; } = "Hall 1";

        [Display(Name = "Total Seats")]
        public int TotalSeats { get; set; } = 100;

        [Display(Name = "Available Seats")]
        public int AvailableSeats { get; set; } = 100;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Ticket Price (RM)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Movie? Movie { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<SeatBooking> SeatBookings { get; set; } = new List<SeatBooking>();
    }
}
