using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using System.Collections.Generic;
using System.Linq;

namespace VEXA.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Admin()
        {
            return View("AdminHome");
        }

        public IActionResult Products()
        {
            List<Product> products = _context.Products.ToList();
            return View("Admin_allproducts", products);
        }

        public IActionResult Users()
        {
            List<User> users = _context.Users.ToList();
            return View("Admin_users", users);
        }

        public IActionResult Orders()
        {
            List<Order> orders = _context.Orders.ToList();
            return View("Admin_Orders", orders);
        }
    }
}
