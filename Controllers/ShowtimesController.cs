using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class ShowtimesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShowtimesController(ApplicationDbContext context) => _context = context;

        // GET: Admin/Showtimes
        [Route("/Admin/Showtimes")]
        public async Task<IActionResult> Index(int? movieId)
        {
            var query = _context.Showtimes
                .Include(s => s.Movie)
                .AsQueryable();

            if (movieId.HasValue)
                query = query.Where(s => s.MovieId == movieId);

            var showtimes = await query.OrderBy(s => s.ShowDate).ThenBy(s => s.ShowTime).ToListAsync();
            ViewBag.Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync();
            ViewBag.MovieFilter = movieId;
            return View(showtimes);
        }

        // GET: Admin/Showtimes/Create
        public async Task<IActionResult> Create(int? movieId)
        {
            var vm = new ShowtimeFormViewModel
            {
                Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync()
            };
            if (movieId.HasValue) vm.MovieId = movieId.Value;
            return View(vm);
        }

        // POST: Admin/Showtimes/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShowtimeFormViewModel model)
        {
            model.Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var showtime = new Showtime
            {
                MovieId = model.MovieId,
                ShowDate = model.ShowDate,
                ShowTime = model.ShowTime,
                Hall = model.Hall,
                TotalSeats = model.TotalSeats,
                AvailableSeats = model.AvailableSeats,
                Price = model.Price,
                IsActive = model.IsActive
            };
            _context.Showtimes.Add(showtime);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Showtime created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Showtimes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var s = await _context.Showtimes.FindAsync(id);
            if (s == null) return NotFound();
            var vm = new ShowtimeFormViewModel
            {
                Id = s.Id,
                MovieId = s.MovieId,
                ShowDate = s.ShowDate,
                ShowTime = s.ShowTime,
                Hall = s.Hall,
                TotalSeats = s.TotalSeats,
                AvailableSeats = s.AvailableSeats,
                Price = s.Price,
                IsActive = s.IsActive,
                Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync()
            };
            return View(vm);
        }

        // POST: Admin/Showtimes/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShowtimeFormViewModel model)
        {
            model.Movies = await _context.Movies.Where(m => m.IsActive).OrderBy(m => m.Title).ToListAsync();
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime == null) return NotFound();

            showtime.MovieId = model.MovieId;
            showtime.ShowDate = model.ShowDate;
            showtime.ShowTime = model.ShowTime;
            showtime.Hall = model.Hall;
            showtime.TotalSeats = model.TotalSeats;
            showtime.AvailableSeats = model.AvailableSeats;
            showtime.Price = model.Price;
            showtime.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Showtime updated.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Showtimes/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.Showtimes.FindAsync(id);
            if (s == null) return NotFound();
            _context.Showtimes.Remove(s);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Showtime deleted.";
            return RedirectToAction("Index");
        }
    }
}
