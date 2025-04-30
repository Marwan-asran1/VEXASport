using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Men()
        {
            return View();
        }

        public IActionResult Women()
        {
            return View();
        }

    }
}
