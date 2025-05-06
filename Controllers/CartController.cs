using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VEXA.Helper;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Cart()
        {
            List<CartItem> cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null)
            {
                cart = new List<CartItem>();
            }
            return View(cart);
        }

        public IActionResult RemoveFromCart(int productId, string size)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { cart = new List<CartItem>(); }
            var removeitem = cart.FirstOrDefault(x => x.ProductId == productId && x.Size == size);
            if (removeitem != null)
            {
                cart.Remove(removeitem);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Cart");
        }

        public IActionResult AddToCart(Product product, string size, int quantity = 1)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existItem = cart.FirstOrDefault(x => x.ProductId == product.Id && x.Size == size);

            if (existItem != null)
            {
                existItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Size = size,
                    Product = product,
                    Quantity = quantity,
                    ImageFileName = product.ImageUrl
                });
            }

            
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateCartItemModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { return Json(new { success = false }); }

            var item = cart.FirstOrDefault(x => x.ProductId == model.ProductId && x.Size == model.Size);
            if (item != null)
            {
                item.Quantity = model.Quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }

    
    public class UpdateCartItemModel
    {
        public int ProductId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
    }
}