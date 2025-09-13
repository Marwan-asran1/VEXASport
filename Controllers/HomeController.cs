using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // First, get all products with their order items
            var productsWithOrders = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.OrderItems)
                .ToList();

            // Calculate best sellers on the client side
            var bestSellingProducts = productsWithOrders
                .Where(p => p.OrderItems != null && p.OrderItems.Any())
                .Select(p => new
                {
                    Product = p,
                    TotalQuantityOrdered = p.OrderItems.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantityOrdered)
                .Take(5)
                .Select(x => x.Product)
                .ToList();

            // If we don't have enough ordered products, fill with newest products
            if (bestSellingProducts.Count < 5)
            {
                var additionalProducts = productsWithOrders
                    .Where(p => !bestSellingProducts.Contains(p))
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5 - bestSellingProducts.Count)
                    .ToList();
                
                bestSellingProducts.AddRange(additionalProducts);
            }

            return View(bestSellingProducts);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
