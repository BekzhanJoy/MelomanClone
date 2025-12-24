using Microsoft.AspNetCore.Mvc;
using MelomanClone.Data;
using Microsoft.EntityFrameworkCore;

namespace MelomanClone.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
