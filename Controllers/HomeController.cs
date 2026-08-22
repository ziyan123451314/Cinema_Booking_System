using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var featured = await _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.Category)
                .OrderByDescending(m => m.CreatedAt)
                .Take(6)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            ViewBag.Categories = categories;
            return View(featured);
        }

        public IActionResult Privacy() => View();
        public IActionResult Error() => View();
    }
}
