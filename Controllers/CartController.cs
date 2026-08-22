using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize(Roles = "Member,Admin")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Showtime)
                        .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var cart = await GetOrCreateCartAsync(user.Id);
            return View(new CartViewModel { Cart = cart, Items = cart.CartItems });
        }

        // POST: /Cart/Add
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddToCartViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId && s.IsActive);

            if (showtime == null)
            {
                TempData["Error"] = "Showtime not found.";
                return RedirectToAction("Index", "Movie");
            }

            if (showtime.AvailableSeats < model.Quantity)
            {
                TempData["Error"] = "Not enough seats available.";
                return RedirectToAction("Detail", "Movie", new { id = showtime.MovieId });
            }

            var cart = await GetOrCreateCartAsync(user.Id);
            var existing = cart.CartItems.FirstOrDefault(ci => ci.ShowtimeId == model.ShowtimeId);
            if (existing != null)
            {
                existing.Quantity += model.Quantity;
                if (existing.Quantity > showtime.AvailableSeats)
                    existing.Quantity = showtime.AvailableSeats;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ShowtimeId = model.ShowtimeId,
                    Quantity = model.Quantity,
                    UnitPrice = showtime.Price
                });
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Added {model.Quantity} ticket(s) for \"{showtime.Movie?.Title}\" to cart.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Update
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Showtime)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == user.Id);

            if (item == null) return NotFound();

            if (quantity <= 0)
                _context.CartItems.Remove(item);
            else
            {
                item.Quantity = Math.Min(quantity, item.Showtime?.AvailableSeats ?? quantity);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cart updated.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/Remove
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == user.Id);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }

        // GET: Cart item count for navbar
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(0);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            return Json(cart?.ItemCount ?? 0);
        }
    }
}
