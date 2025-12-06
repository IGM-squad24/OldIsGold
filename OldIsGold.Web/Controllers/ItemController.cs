using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;
using OldIsGold.Web.Models;

namespace OldIsGold.Web.Controllers
{
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ItemController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Item
        public async Task<IActionResult> Index(string? search, int? categoryId, ItemCondition? condition, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .Where(i => i.Status == ItemStatus.Approved);

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Title.Contains(search) || i.Description.Contains(search));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            // Filter by condition
            if (condition.HasValue)
            {
                query = query.Where(i => i.Condition == condition.Value);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(i => i.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(i => i.Price <= maxPrice.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(i => i.Price),
                "price_desc" => query.OrderByDescending(i => i.Price),
                "newest" => query.OrderByDescending(i => i.CreatedDate),
                "oldest" => query.OrderBy(i => i.CreatedDate),
                "title" => query.OrderBy(i => i.Title),
                _ => query.OrderByDescending(i => i.CreatedDate)
            };

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Condition = condition;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;

            var items = await query.ToListAsync();
            return View(items);
        }

        // GET: Item/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null)
            {
                return NotFound();
            }

            // Increment view count
            item.ViewCount++;
            await _context.SaveChangesAsync();

            // Check if user has wishlisted this item
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.IsWishlisted = await _context.Wishlists
                    .AnyAsync(w => w.ItemId == id && w.UserId == userId);
            }

            // Get related items from same category
            ViewBag.RelatedItems = await _context.Items
                .Include(i => i.Images)
                .Where(i => i.CategoryId == item.CategoryId && i.ItemId != id && i.Status == ItemStatus.Approved)
                .Take(4)
                .ToListAsync();

            return View(item);
        }

        // POST: Item/AddToWishlist/5
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Check if already in wishlist
            var existingWishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ItemId == id && w.UserId == userId);

            if (existingWishlist == null)
            {
                var wishlist = new Wishlist
                {
                    ItemId = id,
                    UserId = userId,
                    AddedDate = DateTime.Now
                };

                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item added to wishlist!";
            }
            else
            {
                TempData["Info"] = "Item is already in your wishlist.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Item/RemoveFromWishlist/5
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ItemId == id && w.UserId == userId);

            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from wishlist!";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Item/Report/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int id, string reason, string? description)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Check if user already reported this item
            var existingReport = await _context.Reports
                .FirstOrDefaultAsync(r => r.ItemId == id && r.ReporterId == userId);

            if (existingReport == null)
            {
                var report = new Report
                {
                    ItemId = id,
                    ReporterId = userId,
                    Reason = reason,
                    Description = description,
                    ReportDate = DateTime.Now,
                    Status = ReportStatus.Pending
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thank you for reporting. We will review this item.";
            }
            else
            {
                TempData["Info"] = "You have already reported this item.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
