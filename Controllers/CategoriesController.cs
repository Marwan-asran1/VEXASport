using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Categories()
        {
            return View();
        }
    }
}
