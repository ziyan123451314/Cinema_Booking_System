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
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            const int pageSize = 9;

            var query = _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search) || (m.Director != null && m.Director.Contains(search)) || (m.Genre != null && m.Genre.Contains(search)));

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(m => m.CategoryId == categoryId);

            var total = await query.CountAsync();
            var movies = await query
                .OrderByDescending(m => m.CreatedAt)
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
    }
}
