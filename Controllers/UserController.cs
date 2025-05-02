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
            // Check if the user exists in the database
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);

            if (existingUser != null)
            {
            
                return RedirectToAction("Home/Index.cshtml"); 
            }
            else
            {
                // Login failed
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View("Login");
            }
        }
    }
}