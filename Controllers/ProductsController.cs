using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using Microsoft.EntityFrameworkCore;

namespace VEXA.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View("Details/Details", product);
        }

        public IActionResult Men()
        {
            var menP = _context.Products.Where(p => p.CategoryId == 1).ToList();
            return View("Men/men", menP);
        }

        public IActionResult Women()
        {
            var womenP = _context.Products.Where(p => p.CategoryId == 2).ToList();
            return View("Women/women", womenP);
        }
        public IActionResult MenTops()
        {
            var menTops = _context.Products.Where(p => p.CategoryId == 3).ToList();
            return View("Men/men", menTops);
        }

        public IActionResult MenBottoms()
        {
            var menBottoms = _context.Products.Where(p => p.CategoryId == 4).ToList();
            return View("Men/men", menBottoms);
        }

        public IActionResult WomenTops()
        {
            var womenTops = _context.Products.Where(p => p.CategoryId == 5).ToList();
            return View("Women/women", womenTops);
        }

        public IActionResult WomenBottoms()
        {
            var womenBottoms = _context.Products.Where(p => p.CategoryId == 6).ToList();
            return View("Women/women", womenBottoms);
        }
        public IActionResult Kids()
        {
            return View();
        }
        public IActionResult AllProducts()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult CreateProduct(Product p)
        {
          
                _context.Products.Add(p);
                _context.SaveChanges();
                return RedirectToAction("Products", "Admin");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Products", "Admin");
        }

    }
}
