using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public async Task<IActionResult> Checkout(int itemId)
        {
            var item = await _context.Items
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.Status == ItemStatus.Approved);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int itemId, string paymentMethod)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _context.Items
                .Include(i => i.Seller)
                .FirstOrDefaultAsync(i => i.ItemId == itemId && i.Status == ItemStatus.Approved);

            if (item == null)
            {
                TempData["Error"] = "Item not found or no longer available.";
                return RedirectToAction("Index", "Item");
            }

            // Check if item is already sold
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.ItemId == itemId && o.Status == OrderStatus.Completed);

            if (existingOrder != null)
            {
                TempData["Error"] = "This item has already been sold.";
                return RedirectToAction("Details", "Item", new { id = itemId });
            }

            // Create order
            var order = new Order
            {
                ItemId = itemId,
                BuyerId = userId,
                SellerId = item.SellerId,
                OrderDate = DateTime.Now,
                TotalAmount = item.Price,
                Status = OrderStatus.Completed, // Dummy payment - instant success
                PaymentMethod = paymentMethod
            };

            _context.Orders.Add(order);

            // Update item status to Sold
            item.Status = ItemStatus.Sold;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful! Your order has been placed.";
            return RedirectToAction(nameof(Confirmation), new { id = order.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Item).ThenInclude(i => i.Images)
                .Include(o => o.Seller)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.BuyerId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
