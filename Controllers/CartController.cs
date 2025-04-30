using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Cart()
        {
            return View();
        }
    }
}
