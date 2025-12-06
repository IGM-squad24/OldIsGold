using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;
using OldIsGold.DAL.Models;

namespace OldIsGold.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var featuredItems = await _context.Items
                .Where(i => i.Status == ItemStatus.Approved)
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Images)
                .OrderByDescending(i => i.CreatedDate)
                .Take(12)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.FeaturedItems = featuredItems;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
