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

        // POST: /Cart/Add (supports AJAX no-refresh & normal form submit)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddToCartViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == model.ShowtimeId && s.IsActive);

            if (showtime == null)
            {
                if (isAjax) return Json(new { success = false, message = "Showtime not found." });
                TempData["Error"] = "Showtime not found.";
                return RedirectToAction("Index", "Movie");
            }

            // Parse chosen seat labels
            var requestedSeats = string.IsNullOrWhiteSpace(model.SeatLabels)
                ? new List<string>()
                : model.SeatLabels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            // If seats were specified by seat map, quantity is derived from seat count
            if (requestedSeats.Count > 0)
                model.Quantity = requestedSeats.Count;

            if (showtime.AvailableSeats < model.Quantity)
            {
                if (isAjax) return Json(new { success = false, message = $"Only {showtime.AvailableSeats} seats available." });
                TempData["Error"] = "Not enough seats available.";
                return RedirectToAction("Detail", "Movie", new { id = showtime.MovieId });
            }

            // Validate no seat conflicts when seat labels are provided
            if (requestedSeats.Count > 0)
            {
                var takenSeats = await _context.SeatBookings
                    .Where(sb => sb.ShowtimeId == model.ShowtimeId && requestedSeats.Contains(sb.SeatLabel))
                    .Select(sb => sb.SeatLabel)
                    .ToListAsync();

                if (takenSeats.Count > 0)
                {
                    var msg = $"Seats {string.Join(", ", takenSeats)} have just been taken. Please select again.";
                    if (isAjax) return Json(new { success = false, message = msg });
                    TempData["Error"] = msg;
                    return RedirectToAction("SeatMap", "Movie", new { showtimeId = model.ShowtimeId });
                }
            }

            var cart = await GetOrCreateCartAsync(user.Id);
            var existing = cart.CartItems.FirstOrDefault(ci => ci.ShowtimeId == model.ShowtimeId);

            if (existing != null)
            {
                // Release old seat holds for this showtime before replacing
                var oldBookings = await _context.SeatBookings
                    .Where(sb => sb.ShowtimeId == model.ShowtimeId && sb.UserId == user.Id && sb.OrderItemId == null)
                    .ToListAsync();
                _context.SeatBookings.RemoveRange(oldBookings);

                existing.Quantity = model.Quantity;
                existing.SeatLabels = model.SeatLabels ?? string.Empty;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ShowtimeId = model.ShowtimeId,
                    Quantity = model.Quantity,
                    UnitPrice = showtime.Price,
                    SeatLabels = model.SeatLabels ?? string.Empty
                });
            }

            // Create temporary seat holds in SeatBookings table
            foreach (var seat in requestedSeats)
            {
                _context.SeatBookings.Add(new SeatBooking
                {
                    ShowtimeId = model.ShowtimeId,
                    SeatLabel = seat,
                    UserId = user.Id,
                    OrderItemId = null // null = temporary cart hold, not yet confirmed
                });
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var successMessage = $"Added {model.Quantity} ticket(s) for \"{showtime.Movie?.Title}\" to cart.";
            if (requestedSeats.Count > 0)
                successMessage = $"Seats {string.Join(", ", requestedSeats)} for \"{showtime.Movie?.Title}\" added to cart!";

            if (isAjax)
            {
                return Json(new {
                    success = true,
                    message = successMessage,
                    cartCount = cart.ItemCount
                });
            }

            TempData["Success"] = successMessage;
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
                item.Quantity = Math.Min(quantity, item.Showtime?.AvailableSeats ?? quantity);

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
                // Release seat holds when removing from cart
                if (!string.IsNullOrEmpty(item.SeatLabels))
                {
                    var heldSeats = item.SeatLabels.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var bookings = await _context.SeatBookings
                        .Where(sb => sb.ShowtimeId == item.ShowtimeId
                                  && sb.UserId == user.Id
                                  && sb.OrderItemId == null
                                  && heldSeats.Contains(sb.SeatLabel))
                        .ToListAsync();
                    _context.SeatBookings.RemoveRange(bookings);
                }

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