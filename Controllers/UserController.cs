using VEXA.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace VEXA.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Verify(User user)
        {
          
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

            if (existingUser != null)
            {
                HttpContext.Session.SetInt32("UserId", existingUser.Id);
                return RedirectToAction("Index","Home"); 
            }
            else
            {
               
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View("User/Login");
            }
        }
    }
}