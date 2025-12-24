using MelomanClone.Data;
using MelomanClone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MelomanClone.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly Cart _cart;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            AppDbContext context,
            IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _cart = new Cart(accessor);
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(_cart);
        }

        public IActionResult Add(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _cart.Add(new CartItem
            {
                ProductId = product.Id,
                Title = product.Title,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl
            });

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            _cart.Remove(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Checkout()
        {
            if (!_cart.Items.Any())
                return RedirectToAction("Index");

            var user = await _userManager.GetUserAsync(User);

            var order = new Order
            {
                UserId = user.Id,
                TotalPrice = _cart.Total(),
                Items = _cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductTitle = i.Title,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            _cart.Clear();

            TempData["Success"] = "Order placed successfully";

            return RedirectToAction("Index", "Home");
        }
    }
}
