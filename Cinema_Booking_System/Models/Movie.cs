using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBookingSystem.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Director { get; set; }

        [MaxLength(100)]
        public string? Cast { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        [Display(Name = "Duration (min)")]
        public int Duration { get; set; }

        [MaxLength(50)]
        public string? Language { get; set; }

        [MaxLength(10)]
        public string? Rating { get; set; }   // PG, PG-13, R, etc.

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Base Price (RM)")]
        public decimal Price { get; set; }

        [Display(Name = "Poster Image")]
        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleasedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual Category? Category { get; set; }
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
