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
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Showtime)
                        .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutViewModel
            {
                ContactName = user.FullName,
                ContactPhone = user.Phone ?? string.Empty,
                ContactEmail = user.Email ?? string.Empty,
                CartItems = cart.CartItems,
                Total = cart.Total
            };

            return View(vm);
        }

        // POST: /Order/PlaceOrder
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Showtime)
                        .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cart.CartItems;
                model.Total = cart.Total;
                return View("Checkout", model);
            }

            // Check seats again
            foreach (var item in cart.CartItems)
            {
                if (item.Showtime != null && item.Showtime.AvailableSeats < item.Quantity)
                {
                    TempData["Error"] = $"Not enough seats for \"{item.Showtime.Movie?.Title}\". Please update your cart.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // Create Order
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Total,
                Status = OrderStatus.Pending,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == PaymentMethod.Cash ? PaymentStatus.Unpaid : PaymentStatus.Paid,
                ContactName = model.ContactName,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                Notes = model.Notes
            };

            foreach (var ci in cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ShowtimeId = ci.ShowtimeId,
                    MovieTitle = ci.Showtime?.Movie?.Title ?? "Unknown",
                    ShowDate = ci.Showtime?.ShowDate ?? DateTime.Today,
                    ShowTime = ci.Showtime?.ShowTime ?? TimeSpan.Zero,
                    Hall = ci.Showtime?.Hall ?? "N/A",
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice
                });

                // Deduct available seats
                if (ci.Showtime != null)
                    ci.Showtime.AvailableSeats -= ci.Quantity;
            }

            _context.Orders.Add(order);

            // Clear cart items
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{order.Id} placed successfully!";
            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        // GET: /Order/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == user!.Id)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(new OrderListViewModel { Orders = orders });
        }

        // GET: /Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Showtime)
                        .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
