using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class ValidationController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
