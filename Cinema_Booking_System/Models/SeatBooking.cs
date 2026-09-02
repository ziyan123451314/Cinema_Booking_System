using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    /// <summary>
    /// Tracks a single booked seat (e.g. "C5") for a specific showtime.
    /// Created when a user adds seats to cart and removed if the cart item is removed.
    /// Permanently linked to an OrderItem once the order is placed.
    /// </summary>
    public class SeatBooking
    {
        public int Id { get; set; }

        public int ShowtimeId { get; set; }

        /// <summary>Row + number label, e.g. "A1", "C10"</summary>
        [Required, MaxLength(10)]
        public string SeatLabel { get; set; } = string.Empty;

        /// <summary>The user who holds this reservation.</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Null while seat is in the cart (temporary hold).
        /// Set to the OrderItem Id once the booking is confirmed.
        /// </summary>
        public int? OrderItemId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual Showtime? Showtime { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual OrderItem? OrderItem { get; set; }
    }
}
