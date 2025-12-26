using Microsoft.AspNetCore.Mvc;
using MelomanClone.Data;
using Microsoft.EntityFrameworkCore;

namespace MelomanClone.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 8;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(
            string keyword,
            int? categoryId,
            string sort,
            int page = 1)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Title.ToLower().Contains(keyword.ToLower()));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p =>
                    p.CategoryId == categoryId);
            }

            productsQuery = sort switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                _ => productsQuery.OrderBy(p => p.Id)
            };

            var totalItems = productsQuery.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            var products = productsQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Sort = sort;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(products);
        }
    }
}
