using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
