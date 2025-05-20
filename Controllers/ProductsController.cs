using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using VEXA.Helper;
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

            if (product == null) { return NotFound(); }

            return View("Details/Details",product);
        }
        public IActionResult Men()
        {
            var menP = _context.Products.Where(p => p.CategoryId == 1).ToList();
            return View("Men/men",menP);
        }
        
        
        public IActionResult Women()
        {
            var womenP=_context.Products.Where(p => p.CategoryId == 2).ToList();
            return View("Women/women",womenP);
        }

        public IActionResult AllProducts()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }


    }
}
