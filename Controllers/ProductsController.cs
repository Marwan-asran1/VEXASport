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
        public IActionResult AddToCart(int id,string size="M")
        {
           var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) { 
                return NotFound();
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { cart = new List<CartItem>(); }
            var exitItem = cart.FirstOrDefault(item => item.ProductId == product.Id && item.Size==size);
            if (exitItem != null) {
                exitItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Size = size,
                    Product = product,
                    Quantity = 1,
                    ImageFileName = product.ImageUrl
                });
            }
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Cart","Cart");
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)  // Include the Category for display
                .FirstOrDefault(p => p.Id == id);

            if (product == null) { return NotFound(); }

            return View("Details/Details",product);
        }
        public IActionResult Men()
        {
            return View("Men/men");
        }
        public IActionResult TopsM()
        {
            return View("Men/TopsM");
        }
        public IActionResult BottomsM()
        {
            return View("Men/BottomsM");
        }
        public IActionResult Women()
        {
            return View("Women/women");
        }
        public IActionResult TopsW()
        {
            return View("Women/TopsW");
        }
        public IActionResult BottomsW()
        {
            return View("Women/BottomsW");
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
