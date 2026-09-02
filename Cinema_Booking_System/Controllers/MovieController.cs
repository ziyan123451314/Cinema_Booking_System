using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context) => _context = context;

        // GET: /Movie  - Product Catalog
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortOrder, int page = 1)
        {
            const int pageSize = 9;

            var query = _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.Category)
                .AsQueryable();

            // 1. 搜索与分类过滤
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search) || (m.Director != null && m.Director.Contains(search)) || (m.Genre != null && m.Genre.Contains(search)));

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(m => m.CategoryId == categoryId);

            // 2. 排序功能逻辑
            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(m => m.Title); // A-Z顺序排列
                    break;
                case "price_asc":
                    query = query.OrderBy(m => m.Price); // 价格 低到高
                    break;
                case "duration_asc":
                    query = query.OrderBy(m => m.Duration); // 电影时长 短到长
                    break;
                default:
                    query = query.OrderByDescending(m => m.CreatedAt); // 默认排序
                    break;
            }

            var total = await query.CountAsync();
            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new MovieCatalogViewModel
            {
                Movies = movies,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                SearchTerm = search,
                CategoryId = categoryId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                PageSize = pageSize
            };

            // 保存排序状态供前端使用
            ViewBag.SortOrder = sortOrder;

            return View(vm);
        }

        // GET: /Movie/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (movie == null) return NotFound();

            var showtimes = await _context.Showtimes
                .Where(s => s.MovieId == id && s.ShowDate >= DateTime.Today && s.IsActive)
                .OrderBy(s => s.ShowDate).ThenBy(s => s.ShowTime)
                .ToListAsync();

            return View(new MovieDetailViewModel { Movie = movie, Showtimes = showtimes });
        }

        // GET: /Movie/SeatMap?showtimeId=X
        /// <summary>
        /// Interactive seat map page for selecting specific seats for a showtime.
        /// </summary>
        public async Task<IActionResult> SeatMap(int showtimeId)
        {
            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == showtimeId && s.IsActive);

            if (showtime == null) return NotFound();

            return View(showtime);
        }

        // GET: /Movie/TakenSeats?showtimeId=X
        /// <summary>
        /// Returns a JSON array of already-booked seat labels for a given showtime.
        /// Used by the seat map JavaScript to mark seats as unavailable.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TakenSeats(int showtimeId)
        {
            var taken = await _context.SeatBookings
                .Where(sb => sb.ShowtimeId == showtimeId)
                .Select(sb => sb.SeatLabel)
                .ToListAsync();

            return Json(taken);
        }
    }
}