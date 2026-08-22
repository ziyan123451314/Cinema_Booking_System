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
    public class MembersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MembersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Admin/Members
        [Route("/Admin/Members")]
        public async Task<IActionResult> Index(string? search)
        {
            var memberRoleId = (await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Member"))?.Id;
            var memberUserIds = memberRoleId != null
                ? await _context.UserRoles.Where(ur => ur.RoleId == memberRoleId).Select(ur => ur.UserId).ToListAsync()
                : new List<string>();

            var query = _context.Users
                .Where(u => memberUserIds.Contains(u.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));

            var members = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

            return View(new MemberListViewModel { Members = members, SearchTerm = search });
        }

        // GET: Admin/Members/Detail/id
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Orders = orders;
            return View(user);
        }

        // POST: Admin/Members/ToggleStatus/id
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Member {user.FullName} has been {(user.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Members/Delete/id
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Check if user has orders
            var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
            if (hasOrders)
            {
                TempData["Error"] = "Cannot delete member with existing orders.";
                return RedirectToAction("Index");
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = $"Member {user.FullName} has been deleted.";
            return RedirectToAction("Index");
        }
    }
}
