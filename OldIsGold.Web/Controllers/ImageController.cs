using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OldIsGold.DAL.Data;

namespace OldIsGold.Web.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> GetImage(int id)
        {
            var image = await _context.ItemImages.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            return File(image.ImageData, image.ContentType);
        }
    }
}
