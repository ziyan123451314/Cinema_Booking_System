using System.ComponentModel.DataAnnotations;

namespace CinemaBookingSystem.Models.ViewModels
{
    // ---- Account ----
    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ---- Catalog ----
    public class MovieCatalogViewModel
    {
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 9;
    }

    public class MovieDetailViewModel
    {
        public Movie Movie { get; set; } = null!;
        public IEnumerable<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }

    // ---- Cart ----
    public class CartViewModel
    {
        public Cart? Cart { get; set; }
        public IEnumerable<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class AddToCartViewModel
    {
        public int ShowtimeId { get; set; }
        [Range(1, 10)] public int Quantity { get; set; } = 1;
    }

    // ---- Checkout ----
    public class CheckoutViewModel
    {
        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string ContactName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email Address")]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public string? Notes { get; set; }

        public IEnumerable<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
    }

    // ---- Order ----
    public class OrderListViewModel
    {
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
    }

    // ---- Admin Dashboard ----
    public class AdminDashboardViewModel
    {
        public int TotalMembers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; } = new List<Order>();
    }

    // ---- Admin Movie ----
    public class MovieFormViewModel
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
        public string? Rating { get; set; }

        [Display(Name = "Base Price (RM)")]
        [Range(0.01, 9999)]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        [Display(Name = "Poster Image")]
        public IFormFile? ImageFile { get; set; }

        [Required, Display(Name = "Category")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Release Date")]
        [DataType(DataType.Date)]
        public DateTime? ReleasedAt { get; set; }

        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    }

    // ---- Admin Showtime ----
    public class ShowtimeFormViewModel
    {
        public int Id { get; set; }

        [Required, Display(Name = "Movie")]
        public int MovieId { get; set; }

        [Required, Display(Name = "Show Date")]
        [DataType(DataType.Date)]
        public DateTime ShowDate { get; set; } = DateTime.Today.AddDays(1);

        [Required, Display(Name = "Show Time")]
        [DataType(DataType.Time)]
        public TimeSpan ShowTime { get; set; } = new TimeSpan(19, 0, 0);

        [Required, MaxLength(50)]
        public string Hall { get; set; } = "Hall 1";

        [Display(Name = "Total Seats"), Range(1, 500)]
        public int TotalSeats { get; set; } = 100;

        [Display(Name = "Available Seats"), Range(0, 500)]
        public int AvailableSeats { get; set; } = 100;

        [Required, Display(Name = "Ticket Price (RM)")]
        [Range(0.01, 9999)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
        public IEnumerable<Movie> Movies { get; set; } = new List<Movie>();
    }

    // ---- Admin Member ----
    public class MemberListViewModel
    {
        public IEnumerable<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();
        public string? SearchTerm { get; set; }
    }

    // ---- Admin Orders ----
    public class AdminOrderListViewModel
    {
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
    }
}
