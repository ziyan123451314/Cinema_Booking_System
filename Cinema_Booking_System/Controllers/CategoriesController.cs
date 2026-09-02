using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context) => _context = context;

        // GET: Admin/Categories
        [Route("/Admin/Categories")]
        public async Task<IActionResult> Index()
        {
            var cats = await _context.Categories
                .Include(c => c.Movies)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(cats);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create() => View(new Category());

        // POST: Admin/Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);
            model.CreatedAt = DateTime.Now;
            _context.Categories.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category \"{model.Name}\" created.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            _context.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category \"{model.Name}\" updated.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.Categories
                .Include(c => c.Movies)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await _context.Categories.Include(c => c.Movies).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();

            if (cat.Movies.Any())
            {
                TempData["Error"] = "Cannot delete category with assigned movies.";
                return RedirectToAction("Index");
            }

            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Categories/ToggleStatus/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            cat.IsActive = !cat.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Category \"{cat.Name}\" {(cat.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction("Index");
        }
    }
}
