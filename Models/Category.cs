using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
