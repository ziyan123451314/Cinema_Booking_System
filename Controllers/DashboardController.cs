using CinemaBookingSystem.Data;
using CinemaBookingSystem.Models;
using CinemaBookingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaBookingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]/{id?}")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Route("/Admin")]
        [Route("/Admin/Dashboard")]
        [Route("/Admin/Dashboard/Index")]
        public async Task<IActionResult> Index()
        {
            var memberRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Member");
            int memberCount = 0;
            if (memberRole != null)
                memberCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == memberRole.Id);

            var vm = new AdminDashboardViewModel
            {
                TotalMembers = memberCount,
                TotalMovies = await _context.Movies.CountAsync(m => m.IsActive),
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount),
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
