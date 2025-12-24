using Microsoft.AspNetCore.Mvc;
using MelomanClone.Data;
using Microsoft.EntityFrameworkCore;

namespace MelomanClone.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();

            return View(products);
        }
    }
}
