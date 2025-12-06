using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;

namespace OldIsGold.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            // Get item counts for each category
            foreach (var category in categories)
            {
                category.Items = await _context.Items
                    .Where(i => i.CategoryId == category.CategoryId && i.Status == DAL.Models.ItemStatus.Approved)
                    .ToListAsync();
            }

            return View(categories);
        }

        public async Task<IActionResult> Items(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var items = await _context.Items
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .Where(i => i.CategoryId == id && i.Status == DAL.Models.ItemStatus.Approved)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            ViewBag.CategoryName = category.Name;
            ViewBag.CategoryId = id;

            return View(items);
        }
    }
}
