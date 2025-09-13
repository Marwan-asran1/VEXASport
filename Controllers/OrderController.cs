using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Helper;
using VEXA.Models;
using Microsoft.AspNetCore.Authorization;

namespace VEXA.Controllers
{
    [Authorize] // Require authentication for all order operations
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Cart", "Cart");
            }

            // Ensure each cart item has its Product loaded for display safety
            foreach (var item in cart)
            {
                if (item.Product == null)
                {
                    item.Product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                }
            }

            // Get user information for saved address and phone
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    ViewBag.UserAddress = user.Address;
                    ViewBag.UserPhone = user.PhoneNumber;
                }
            }

            return View(cart);
        }

        [HttpPost]
        public IActionResult PlaceOrder(string address, string contactPhone)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (cart == null || !cart.Any())
                return RedirectToAction("Cart", "Cart");

            // Get authenticated user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Validate stock availability before creating order
            var stockValidationErrors = new List<string>();
            var productsToUpdate = new List<(ProductVariant variant, int quantity)>();

            foreach (var item in cart)
            {
                var product = _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product == null)
                {
                    stockValidationErrors.Add($"Product {item.Product?.Name} is no longer available");
                    continue;
                }

                var variant = product.Variants?.FirstOrDefault(v => v.Size == item.Size);
                if (variant == null)
                {
                    stockValidationErrors.Add($"{product.Name} in size {item.Size} is no longer available");
                    continue;
                }

                if (variant.StockQuantity < item.Quantity)
                {
                    stockValidationErrors.Add($"Only {variant.StockQuantity} items available for {product.Name} in size {item.Size}");
                    continue;
                }

                productsToUpdate.Add((variant, item.Quantity));
            }

            // If there are stock validation errors, return to cart with error message
            if (stockValidationErrors.Any())
            {
                TempData["Error"] = "Order cannot be placed due to stock issues: " + string.Join(", ", stockValidationErrors);
                return RedirectToAction("Cart", "Cart");
            }

            // Create the order
            // Calculate subtotal by loading products and getting their prices
            decimal subtotal = 0;
            foreach (var item in cart)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    subtotal += (product.Price * item.Quantity);
                }
            }
            
            var deliveryFee = 50.00m;
            var totalWithDelivery = subtotal + deliveryFee;
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"PlaceOrder - Subtotal: {subtotal}");
            System.Diagnostics.Debug.WriteLine($"PlaceOrder - Delivery Fee: {deliveryFee}");
            System.Diagnostics.Debug.WriteLine($"PlaceOrder - Total with Delivery: {totalWithDelivery}");
            
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = address,
                BillingAddress = address, // Use same address for both
                ContactPhone = contactPhone,
                Method = Order.PaymentMethod.CashOnDelivery,
                OrderTotal = totalWithDelivery,
                Status = Order.OrderStatus.Confirmed, // Orders are automatically confirmed
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                // Ensure we have the product loaded with price
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                var unitPrice = product?.Price ?? 0;
                
                // Debug logging for each order item
                System.Diagnostics.Debug.WriteLine($"OrderItem - ProductId: {item.ProductId}, Product: {product?.Name}, UnitPrice: {unitPrice}, Quantity: {item.Quantity}");
                
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Size = item.Size
                };
                order.OrderItems.Add(orderItem);
            }

            // Reduce stock quantities
            foreach (var (variant, quantity) in productsToUpdate)
            {
                variant.StockQuantity -= quantity;
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Debug logging after save
            System.Diagnostics.Debug.WriteLine($"After Save - Order ID: {order.Id}");
            System.Diagnostics.Debug.WriteLine($"After Save - OrderTotal: {order.OrderTotal}");
            
            // Verify the order was saved correctly by retrieving it
            var savedOrder = _context.Orders.Find(order.Id);
            if (savedOrder != null)
            {
                System.Diagnostics.Debug.WriteLine($"Retrieved Order - OrderTotal: {savedOrder.OrderTotal}");
            }

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"OrderConfirmation - Order ID: {order.Id}");
            System.Diagnostics.Debug.WriteLine($"OrderConfirmation - OrderTotal: {order.OrderTotal}");
            System.Diagnostics.Debug.WriteLine($"OrderConfirmation - OrderItems Count: {order.OrderItems?.Count ?? 0}");
            
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    System.Diagnostics.Debug.WriteLine($"OrderItem - Product: {item.Product?.Name}, UnitPrice: {item.UnitPrice}, Quantity: {item.Quantity}");
                }
            }

            return View(order);
        }

        public IActionResult MyOrders()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "User");
            }

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"MyOrders - User ID: {userId}");
            System.Diagnostics.Debug.WriteLine($"MyOrders - Orders Count: {orders?.Count ?? 0}");
            
            if (orders != null)
            {
                foreach (var order in orders)
                {
                    System.Diagnostics.Debug.WriteLine($"Order {order.Id}: OrderItems Count = {order.OrderItems?.Count ?? 0}");
                    if (order.OrderItems != null)
                    {
                        foreach (var item in order.OrderItems)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Item: ProductId={item.ProductId}, Product={item.Product?.Name ?? "NULL"}, UnitPrice={item.UnitPrice}");
                        }
                    }
                }
            }

            return View(orders);
        }
    }
}
