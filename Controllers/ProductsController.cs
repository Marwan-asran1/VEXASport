using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View("Details/Details", product);
        }

        public IActionResult Men()
        {
            var menP = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Men)
                .ToList();
            return View("Men/men", menP);
        }

        public IActionResult Women()
        {
            var womenP = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Women)
                .ToList();
            return View("Women/women", womenP);
        }
        
        public IActionResult MenTops()
        {
            var menTops = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Men && p.ProductType == ProductType.Tops)
                .ToList();
            return View("Men/men", menTops);
        }

        public IActionResult MenBottoms()
        {
            var menBottoms = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Men && p.ProductType == ProductType.Bottoms)
                .ToList();
            return View("Men/men", menBottoms);
        }

        public IActionResult WomenTops()
        {
            var womenTops = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Women && p.ProductType == ProductType.Tops)
                .ToList();
            return View("Women/women", womenTops);
        }

        public IActionResult WomenBottoms()
        {
            var womenBottoms = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Women && p.ProductType == ProductType.Bottoms)
                .ToList();
            return View("Women/women", womenBottoms);
        }
        
        public IActionResult Kids()
        {
            var kidsP = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Kids)
                .ToList();
            return View("Kids/kids", kidsP);
        }
        
        public IActionResult KidsTops()
        {
            var kidsTops = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Kids && p.ProductType == ProductType.Tops)
                .ToList();
            return View("Kids/kids", kidsTops);
        }

        public IActionResult KidsBottoms()
        {
            var kidsBottoms = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == Gender.Kids && p.ProductType == ProductType.Bottoms)
                .ToList();
            return View("Kids/kids", kidsBottoms);
        }
        
        public IActionResult AllProducts()
        {
            var products = _context.Products
                .Include(p => p.Variants)
                .ToList();
            return View(products);
        }

        // Dynamic category action that handles all genders
        public IActionResult Category(string gender)
        {
            if (string.IsNullOrEmpty(gender))
            {
                return RedirectToAction("AllProducts");
            }

            // Parse gender from string to enum
            if (!Enum.TryParse<Gender>(gender, true, out var genderEnum))
            {
                return NotFound();
            }

            var products = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == genderEnum)
                .ToList();

            ViewBag.CategoryName = gender;
            ViewBag.Gender = genderEnum;
            
            return View("Category", products);
        }

        // Dynamic subcategory action (tops/bottoms)
        public IActionResult SubCategory(string gender, string productType)
        {
            _logger.LogInformation("SubCategory called with gender: {Gender}, productType: {ProductType}", gender, productType);
            
            if (string.IsNullOrEmpty(gender) || string.IsNullOrEmpty(productType))
            {
                _logger.LogWarning("Missing gender or productType parameters - gender: '{Gender}', productType: '{ProductType}', redirecting to AllProducts", gender, productType);
                return RedirectToAction("AllProducts");
            }

            // Parse gender and product type from strings to enums
            if (!Enum.TryParse<Gender>(gender, true, out var genderEnum) ||
                !Enum.TryParse<ProductType>(productType, true, out var productTypeEnum))
            {
                _logger.LogWarning("Failed to parse enums: gender={Gender}, productType={ProductType}", gender, productType);
                return NotFound();
            }

            var products = _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Gender == genderEnum && p.ProductType == productTypeEnum)
                .ToList();

            _logger.LogInformation("Found {Count} products for gender={Gender}, productType={ProductType}", 
                products.Count, genderEnum, productTypeEnum);

            ViewBag.CategoryName = gender;
            ViewBag.SubCategoryName = productType;
            ViewBag.Gender = genderEnum;
            ViewBag.ProductType = productTypeEnum;
            
            return View("Category", products);
        }

        // Get product data for editing via AJAX
        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            return Json(product);
        }

        // Create new product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product p)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Products.Add(p);
            _context.SaveChanges();

            return RedirectToAction("AllProducts");
        }

        // Edit existing product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product p)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingProduct = _context.Products.Find(p.Id);
            if (existingProduct == null)
                return NotFound();

            existingProduct.Name = p.Name;
            existingProduct.Description = p.Description;
            existingProduct.Price = p.Price;
            existingProduct.ImageUrl = p.ImageUrl;
            existingProduct.Gender = p.Gender;
            existingProduct.ProductType = p.ProductType;
            // Size is now managed through ProductVariants, not directly on Product

            _context.SaveChanges();

            return RedirectToAction("AllProducts");
        }

        // Delete product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.OrderItems)
                    .FirstOrDefault(p => p.Id == id);
                    
                if (product == null)
                    return NotFound();

                // Check if product has any order items and log warning
                if (product.OrderItems != null && product.OrderItems.Any())
                {
                    _logger.LogWarning("Deleting product {ProductId} ({ProductName}) that has {OrderCount} order items. Order records will be preserved.", 
                        product.Id, product.Name, product.OrderItems.Count);
                }
                
                // Remove all variants first
                if (product.Variants != null && product.Variants.Any())
                {
                    _context.ProductVariants.RemoveRange(product.Variants);
                }
                
                // Remove the product (OrderItems will be preserved due to foreign key constraints)
                _context.Products.Remove(product);
                _context.SaveChanges();
                
                TempData["Success"] = "Product deleted successfully! Order records have been preserved.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete product: {ex.Message}";
            }
            
            return RedirectToAction("AllProducts");
        }

        // Search functionality
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("AllProducts");
            }

            // First, get all products with variants
            var allProducts = _context.Products
                .Include(p => p.Variants)
                .ToList();

            // Then filter on the client side to avoid EF translation issues
            var products = allProducts.Where(p => 
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.ProductType.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Gender.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.SearchQuery = query;
            ViewBag.ResultsCount = products.Count;
            
            return View("SearchResults", products);
        }
    }
}
