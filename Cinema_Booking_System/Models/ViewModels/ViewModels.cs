using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

    // 新增：重置/修改密码 ViewModel
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password), MinLength(6, ErrorMessage = "New password must be at least 6 characters.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match.")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    // 新增：整合个人信息与修改密码的 Profile 页面模型
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public ChangePasswordViewModel ChangePassword { get; set; } = new ChangePasswordViewModel();
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
        [Range(1, 20)] public int Quantity { get; set; } = 1;

        /// <summary>Comma-separated seat labels, e.g. "A1,A2". Empty if no seat map used.</summary>
        public string SeatLabels { get; set; } = string.Empty;
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

        // 新增：用于接收前端裁剪、旋转后的 Base64 图片字符串
        public string? CroppedImageBase64 { get; set; }

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