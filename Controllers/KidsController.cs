using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class KidsController : Controller
    {
        public IActionResult Kids()
        {
            return View();
        }
    }
}
