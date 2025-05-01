using Microsoft.AspNetCore.Mvc;
using VEXA.Models;

namespace VEXA.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Verify(User user) { 
        
            return View();
        }
    }
}
