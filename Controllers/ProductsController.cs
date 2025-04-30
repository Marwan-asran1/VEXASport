using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class ProductsController : Controller
    {
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
            return View();
        }

    }
}
