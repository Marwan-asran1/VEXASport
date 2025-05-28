using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Helper;
using VEXA.Models;
using System.Collections.Generic;
using System.Linq;

namespace VEXA.Controllers
{
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
            return View(cart);
        }

        
        [HttpPost]
        public IActionResult PlaceOrder(string shippingAddress, string billingAddress, string contactPhone)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");

            if (cart == null || !cart.Any())
                return RedirectToAction("Cart", "Cart");

            // Temporarily commenting out login check
            /*
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                HttpContext.Session.Remove("Cart");
                return RedirectToAction("Login", "User");
            }
            */

            // Get the existing user 'karem'
            var user = _context.Users.FirstOrDefault(u => u.Email == "karem@karem");
            if (user == null)
            {
                return RedirectToAction("Cart", "Cart");
            }

            var order = new Order
            {
                UserId = user.Id, // Use karem's user ID
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                ContactPhone = contactPhone,
                Method = Order.PaymentMethod.CashOnDelivery,
                OrderTotal = cart.Sum(item => item.TotalPrice),
                Status = Order.OrderStatus.Confirmed,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                };
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }


        public IActionResult OrderConfirmation(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}
