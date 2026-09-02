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
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MoviesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Movies
        [Route("/Admin/Movies")]
        public async Task<IActionResult> Index(string? search, string? sortOrder, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.Movies.Include(m => m.Category).AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search) || (m.Director != null && m.Director.Contains(search)));

            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(m => m.Title);
                    break;
                case "price_asc":
                    query = query.OrderBy(m => m.Price);
                    break;
                case "duration_asc":
                    query = query.OrderBy(m => m.Duration);
                    break;
                default:
                    query = query.OrderByDescending(m => m.CreatedAt);
                    break;
            }

            var total = await query.CountAsync();
            var movies = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            
            return View(movies);
        }

        // GET: Admin/Movies/Create
        public async Task<IActionResult> Create()
        {
            var vm = new MovieFormViewModel
            {
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync()
            };
            return View(vm);
        }

        // POST: Admin/Movies/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var movie = new Movie
            {
                Title = model.Title,
                Description = model.Description,
                Director = model.Director,
                Cast = model.Cast,
                Genre = model.Genre,
                Duration = model.Duration,
                Language = model.Language,
                Rating = model.Rating,
                Price = model.Price,
                CategoryId = model.CategoryId,
                IsActive = model.IsActive,
                ReleasedAt = model.ReleasedAt,
                CreatedAt = DateTime.Now
            };

            // 优先处理前端裁剪后的 Base64 图片数据
            if (!string.IsNullOrEmpty(model.CroppedImageBase64))
            {
                try
                {
                    var base64Data = model.CroppedImageBase64.Substring(model.CroppedImageBase64.IndexOf(",") + 1);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "movies");
                    Directory.CreateDirectory(uploadDir);
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(uploadDir, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    movie.ImagePath = $"/uploads/movies/{fileName}";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("ImageFile", "Failed to process cropped image.");
                    return View(model);
                }
            }
            else if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Only image files (jpg, png, gif, webp) are allowed.");
                    return View(model);
                }

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "movies");
                Directory.CreateDirectory(uploadDir);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                movie.ImagePath = $"/uploads/movies/{fileName}";
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" created successfully.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Movies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var vm = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                Director = movie.Director,
                Cast = movie.Cast,
                Genre = movie.Genre,
                Duration = movie.Duration,
                Language = movie.Language,
                Rating = movie.Rating,
                Price = movie.Price,
                ImagePath = movie.ImagePath,
                CategoryId = movie.CategoryId,
                IsActive = movie.IsActive,
                ReleasedAt = movie.ReleasedAt,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync()
            };
            return View(vm);
        }

        // POST: Admin/Movies/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovieFormViewModel model)
        {
            model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.Title = model.Title;
            movie.Description = model.Description;
            movie.Director = model.Director;
            movie.Cast = model.Cast;
            movie.Genre = model.Genre;
            movie.Duration = model.Duration;
            movie.Language = model.Language;
            movie.Rating = model.Rating;
            movie.Price = model.Price;
            movie.CategoryId = model.CategoryId;
            movie.IsActive = model.IsActive;
            movie.ReleasedAt = model.ReleasedAt;

            // 优先处理前端裁剪后的 Base64 图片数据
            if (!string.IsNullOrEmpty(model.CroppedImageBase64))
            {
                try
                {
                    if (!string.IsNullOrEmpty(movie.ImagePath))
                    {
                        var oldPath = Path.Combine(_env.WebRootPath, movie.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var base64Data = model.CroppedImageBase64.Substring(model.CroppedImageBase64.IndexOf(",") + 1);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "movies");
                    Directory.CreateDirectory(uploadDir);
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(uploadDir, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    movie.ImagePath = $"/uploads/movies/{fileName}";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("ImageFile", "Failed to process cropped image.");
                    return View(model);
                }
            }
            else if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageFile", "Only image files (jpg, png, gif, webp) are allowed.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(movie.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, movie.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "movies");
                Directory.CreateDirectory(uploadDir);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);
                movie.ImagePath = $"/uploads/movies/{fileName}";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" updated.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Movies/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        // POST: Admin/Movies/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            if (!string.IsNullOrEmpty(movie.ImagePath))
            {
                var oldPath = Path.Combine(_env.WebRootPath, movie.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" deleted.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Movies/ToggleStatus/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();
            movie.IsActive = !movie.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Movie \"{movie.Title}\" {(movie.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction("Index");
        }

        // GET: Admin/Movies/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Category)
                .Include(m => m.Showtimes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }
    }
}