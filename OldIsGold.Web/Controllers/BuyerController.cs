using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Controllers
{
    [Authorize(Roles = "Buyer")]
    public class BuyerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BuyerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            var wishlistCount = await _context.Wishlists.CountAsync(w => w.UserId == userId);
            var ordersCount = await _context.Orders.CountAsync(o => o.BuyerId == userId);
            var totalSpent = await _context.Orders
                .Where(o => o.BuyerId == userId && o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentOrders = await _context.Orders
                .Include(o => o.Item).ThenInclude(i => i.Images)
                .Include(o => o.Seller)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.WishlistCount = wishlistCount;
            ViewBag.OrdersCount = ordersCount;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

        public async Task<IActionResult> Wishlist()
        {
            var userId = _userManager.GetUserId(User);

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Item).ThenInclude(i => i.Category)
                .Include(w => w.Item).ThenInclude(i => i.Seller)
                .Include(w => w.Item).ThenInclude(i => i.Images)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return View(wishlistItems);
        }

        public async Task<IActionResult> Orders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Include(o => o.Item).ThenInclude(i => i.Images)
                .Include(o => o.Seller)
                .Where(o => o.BuyerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}
