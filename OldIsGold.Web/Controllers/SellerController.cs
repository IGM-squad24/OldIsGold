using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;
using OldIsGold.Web.Models;

namespace OldIsGold.Web.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SellerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            // Get seller stats
            var totalItems = await _context.Items.CountAsync(i => i.SellerId == userId);
            var pendingItems = await _context.Items.CountAsync(i => i.SellerId == userId && i.Status == ItemStatus.Pending);
            var approvedItems = await _context.Items.CountAsync(i => i.SellerId == userId && i.Status == ItemStatus.Approved);
            var soldItems = await _context.Items.CountAsync(i => i.SellerId == userId && i.Status == ItemStatus.Sold);
            
            var totalSales = await _context.Orders
                .Where(o => o.SellerId == userId && o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentSales = await _context.Orders
                .Include(o => o.Item)
                .Include(o => o.Buyer)
                .Where(o => o.SellerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalItems = totalItems;
            ViewBag.PendingItems = pendingItems;
            ViewBag.ApprovedItems = approvedItems;
            ViewBag.SoldItems = soldItems;
            ViewBag.TotalSales = totalSales;
            ViewBag.Rating = user?.Rating ?? 0;
            ViewBag.RecentSales = recentSales;

            return View();
        }

        public async Task<IActionResult> MyListings(ItemStatus? status)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Images)
                .Where(i => i.SellerId == userId);

            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            var items = await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
            ViewBag.StatusFilter = status;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> CreateItem()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(CreateItemViewModel model, List<IFormFile>? images)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var item = new Item
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    Condition = model.Condition,
                    Year = model.Year,
                    CategoryId = model.CategoryId,
                    SellerId = userId,
                    CreatedDate = DateTime.Now,
                    Status = ItemStatus.Pending
                };

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Handle image uploads
                if (images != null && images.Count > 0)
                {
                    bool isFirstImage = true;
                    foreach (var image in images.Take(5)) // Limit to 5 images
                    {
                        if (image.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await image.CopyToAsync(memoryStream);

                            var itemImage = new ItemImage
                            {
                                ItemId = item.ItemId,
                                ImageData = memoryStream.ToArray(),
                                ContentType = image.ContentType,
                                FileName = image.FileName,
                                IsMain = isFirstImage
                            };

                            _context.ItemImages.Add(itemImage);
                            isFirstImage = false;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Item created successfully! It is pending admin approval.";
                return RedirectToAction(nameof(MyListings));
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.Items
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.ItemId == id && i.SellerId == userId);

            if (item == null)
            {
                return NotFound();
            }

            var model = new EditItemViewModel
            {
                ItemId = item.ItemId,
                Title = item.Title,
                Description = item.Description,
                Price = item.Price,
                Condition = item.Condition,
                Year = item.Year,
                CategoryId = item.CategoryId
            };

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.ExistingImages = item.Images;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(EditItemViewModel model, List<IFormFile>? newImages, List<int>? deleteImageIds)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var item = await _context.Items
                    .Include(i => i.Images)
                    .FirstOrDefaultAsync(i => i.ItemId == model.ItemId && i.SellerId == userId);

                if (item == null)
                {
                    return NotFound();
                }

                item.Title = model.Title;
                item.Description = model.Description;
                item.Price = model.Price;
                item.Condition = model.Condition;
                item.Year = model.Year;
                item.CategoryId = model.CategoryId;

                // Delete selected images
                if (deleteImageIds != null && deleteImageIds.Count > 0)
                {
                    var imagesToDelete = item.Images.Where(img => deleteImageIds.Contains(img.ImageId)).ToList();
                    _context.ItemImages.RemoveRange(imagesToDelete);
                }

                // Add new images
                if (newImages != null && newImages.Count > 0)
                {
                    bool hasMainImage = item.Images.Any(img => img.IsMain && (deleteImageIds == null || !deleteImageIds.Contains(img.ImageId)));
                    
                    foreach (var image in newImages.Take(5))
                    {
                        if (image.Length > 0)
                        {
                            using var memoryStream = new MemoryStream();
                            await image.CopyToAsync(memoryStream);

                            var itemImage = new ItemImage
                            {
                                ItemId = item.ItemId,
                                ImageData = memoryStream.ToArray(),
                                ContentType = image.ContentType,
                                FileName = image.FileName,
                                IsMain = !hasMainImage
                            };

                            _context.ItemImages.Add(itemImage);
                            hasMainImage = true;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Item updated successfully!";
                return RedirectToAction(nameof(MyListings));
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var userId = _userManager.GetUserId(User);
            var item = await _context.Items
                .Include(i => i.Images)
                .FirstOrDefaultAsync(i => i.ItemId == id && i.SellerId == userId);

            if (item == null)
            {
                return NotFound();
            }

            // Check if item has orders
            var hasOrders = await _context.Orders.AnyAsync(o => o.ItemId == id);
            if (hasOrders)
            {
                TempData["Error"] = "Cannot delete item with existing orders.";
                return RedirectToAction(nameof(MyListings));
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Item deleted successfully!";
            return RedirectToAction(nameof(MyListings));
        }

        public async Task<IActionResult> Sales()
        {
            var userId = _userManager.GetUserId(User);
            
            var sales = await _context.Orders
                .Include(o => o.Item)
                .Include(o => o.Buyer)
                .Where(o => o.SellerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(sales);
        }
    }
}
