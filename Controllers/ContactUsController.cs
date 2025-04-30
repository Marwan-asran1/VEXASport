using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class ContactUsController : Controller
    {
        public IActionResult Contact()
        {
            return View();
        }
    }
}
