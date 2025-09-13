using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Helper;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Cart()
        {
            Console.WriteLine("=== CART CONTROLLER START ===");
            
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            
            // Debug: Log cart information
            Console.WriteLine($"Cart has {cart.Count} items");
            
            if (cart.Count == 0)
            {
                Console.WriteLine("Cart is empty, returning empty view");
                ViewBag.CartCount = 0;
                ViewBag.CartTotal = 0;
                return View(new List<CartItem>());
            }
            
            // Get all product IDs from cart
            var productIds = cart.Select(item => item.ProductId).ToList();
            Console.WriteLine($"Product IDs in cart: [{string.Join(", ", productIds)}]");
            
            // Load all products in one query
            var products = _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionary(p => p.Id, p => p);
            
            Console.WriteLine($"Loaded {products.Count} products from database");
            foreach (var product in products.Values)
            {
                Console.WriteLine($"  Product: ID={product.Id}, Name={product.Name}, Price={product.Price}");
            }
            
            // Create a new list with loaded products
            var cartWithProducts = new List<CartItem>();
            decimal calculatedTotal = 0;
            
            foreach (var item in cart)
            {
                Console.WriteLine($"Processing cart item: ProductId={item.ProductId}, Quantity={item.Quantity}");
                
                var product = products.GetValueOrDefault(item.ProductId);
                if (product != null)
                {
                    item.Product = product;
                    var itemTotal = product.Price * item.Quantity;
                    calculatedTotal += itemTotal;
                    
                    Console.WriteLine($"  SUCCESS: Product={product.Name}, Price={product.Price}, Quantity={item.Quantity}, ItemTotal={itemTotal}");
                }
                else
                {
                    Console.WriteLine($"  ERROR: Product with ID {item.ProductId} not found in database!");
                }
                
                cartWithProducts.Add(item);
            }
            
            Console.WriteLine($"Final calculated total: {calculatedTotal}");
            Console.WriteLine($"ViewBag.CartTotal will be set to: {calculatedTotal}");
            
            // Set cart count and total in ViewBag for JavaScript and view
            ViewBag.CartCount = cart.Sum(item => item.Quantity);
            ViewBag.CartTotal = calculatedTotal;
            
            Console.WriteLine("=== CART CONTROLLER END ===");
            
            return View(cartWithProducts);
        }

        public IActionResult RemoveFromCart(int productId/*, string size*/)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { cart = new List<CartItem>(); }
            var removeitem = cart.FirstOrDefault(x => x.ProductId == productId /*&& x.Size == size*/);
            if (removeitem != null)
            {
                cart.Remove(removeitem);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Cart");
        }

        public IActionResult AddToCart(int id, string size)
        {
            try
            {
                if (string.IsNullOrEmpty(size))
                {
                    return Json(new { success = false, message = "Please select a size" });
                }

                var product = _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefault(p => p.Id == id);
                if (product == null) { 
                    return Json(new { success = false, message = "Product not found" });
                }

                if (!Enum.TryParse<ClothingSize>(size, true, out var parsedSize))
                {
                    return Json(new { success = false, message = "Invalid size selected" });
                }

                // Debug: Log product and variant information
                Console.WriteLine($"AddToCart - Product ID: {id}, Size: {size}, Parsed Size: {parsedSize}");
                Console.WriteLine($"Product has {product.Variants?.Count ?? 0} variants");
                
                if (product.Variants != null)
                {
                    foreach (var v in product.Variants)
                    {
                        Console.WriteLine($"Variant: Size={v.Size}, Stock={v.StockQuantity}");
                    }
                }

                // Check if product has any variants
                if (product.Variants == null || !product.Variants.Any())
                {
                    return Json(new { success = false, message = "This product has no size variants available. Please contact admin." });
                }

                // Check stock availability
                var variant = product.Variants.FirstOrDefault(v => v.Size == parsedSize);
                if (variant == null)
                {
                    var availableSizes = string.Join(", ", product.Variants.Select(v => v.Size.ToString()));
                    return Json(new { success = false, message = $"Size {parsedSize} is not available. Available sizes: {availableSizes}" });
                }

                Console.WriteLine($"Found variant: Size={variant.Size}, Stock={variant.StockQuantity}");

                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
                if (cart == null) { cart = new List<CartItem>(); }

                var existItem = cart.FirstOrDefault(item => item.ProductId == product.Id && item.Size == parsedSize);
                int requestedQuantity = (existItem?.Quantity ?? 0) + 1;

                Console.WriteLine($"Existing item quantity: {existItem?.Quantity ?? 0}, Requested: {requestedQuantity}");

                // Check if adding this item would exceed available stock
                if (requestedQuantity > variant.StockQuantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Only {variant.StockQuantity} items available in stock" 
                    });
                }

                if (existItem != null) {
                    existItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Size = parsedSize,
                        Quantity = 1
                    });
                }

                HttpContext.Session.SetObjectAsJson("Cart", cart);
                int totalItemsInCart = cart.Sum(x => x.Quantity);
                return Json(new {
                    success = true,
                    cartcounter = totalItemsInCart,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToCart error: {ex.Message}");
                return Json(new { success = false, message = $"Error adding to cart: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var count = cart.Sum(item => item.Quantity);
            return Json(new { count = count });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity, string size)
        {
            if (quantity < 1)
            {
                return Json(new { success = false, message = "Quantity must be at least 1" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null)
            {
                return Json(new { success = false, message = "Cart not found" });
            }

            var item = cart.FirstOrDefault(x => x.ProductId == productId && x.Size.ToString() == size);
            if (item == null)
            {
                return Json(new { success = false, message = "Item not found in cart" });
            }

            // Check stock availability
            var product = _context.Products
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == productId);
            
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            if (!Enum.TryParse<ClothingSize>(size, true, out var parsedSize))
            {
                return Json(new { success = false, message = "Invalid size" });
            }

            var variant = product.Variants?.FirstOrDefault(v => v.Size == parsedSize);
            if (variant == null)
            {
                return Json(new { success = false, message = "This size is not available" });
            }

            // Check if requested quantity exceeds available stock
            if (quantity > variant.StockQuantity)
            {
                return Json(new { 
                    success = false, 
                    message = $"Only {variant.StockQuantity} items available in stock" 
                });
            }

            item.Quantity = quantity;
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Json(new { success = true });
        }
    }

    

}