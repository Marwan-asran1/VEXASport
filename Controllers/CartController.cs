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

        public IActionResult AddToCart(int id/*, string size="M"*/)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) { 
                return NotFound();
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null) { cart = new List<CartItem>(); }
            var exitItem = cart.FirstOrDefault(item => item.ProductId == product.Id/* && item.Size==size*/);
            if (exitItem != null) {
                exitItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    //Size = size,
                    Product = product,
                    Quantity = 1,
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

            var item = cart.FirstOrDefault(x => x.ProductId == model.ProductId/* && x.Size == model.Size*/);
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
        //public string Size { get; set; }
        public int Quantity { get; set; }
    }
}