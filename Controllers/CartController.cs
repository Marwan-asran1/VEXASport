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
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            return View(cart);
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
            if (string.IsNullOrEmpty(size))
            {
                return Json(new { success = false, message = "Please select a size" });
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) { 
                return NotFound();
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { cart = new List<CartItem>(); }

            var existItem = cart.FirstOrDefault(item => item.ProductId == product.Id && item.Size == size);
            if (existItem != null) {
                existItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Size = size,
                    Product = product,
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
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null)
            {
                return Json(new { success = false });
            }

            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }

    

}