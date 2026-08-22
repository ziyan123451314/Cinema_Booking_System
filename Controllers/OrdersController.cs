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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context) => _context = context;

        // GET: Admin/Orders
        [Route("/Admin/Orders")]
        public async Task<IActionResult> Index(string? status, string? search)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var parsedStatus))
                query = query.Where(o => o.Status == parsedStatus);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.User!.FullName.Contains(search) || o.ContactEmail.Contains(search));

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(new AdminOrderListViewModel { Orders = orders, StatusFilter = status, SearchTerm = search });
        }

        // GET: Admin/Orders/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Showtime)
                        .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.Status = status;
            if (status == OrderStatus.Completed)
                order.PaymentStatus = PaymentStatus.Paid;
            else if (status == OrderStatus.Cancelled)
            {
                // Restore seats
                var items = await _context.OrderItems
                    .Include(oi => oi.Showtime)
                    .Where(oi => oi.OrderId == id)
                    .ToListAsync();
                foreach (var item in items)
                    if (item.Showtime != null) item.Showtime.AvailableSeats += item.Quantity;
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{id} status updated to {status}.";
            return RedirectToAction("Detail", new { id });
        }

        // POST: Admin/Orders/UpdatePayment/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePayment(int id, PaymentStatus paymentStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.PaymentStatus = paymentStatus;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{id} payment status updated to {paymentStatus}.";
            return RedirectToAction("Detail", new { id });
        }
    }
}
