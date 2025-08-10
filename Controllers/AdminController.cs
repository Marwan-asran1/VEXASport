using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            List<Product> products = _context.Products.Include(p => p.Category).ToList();
            return View("Admin_allproducts", products);
        }

        public IActionResult Users()
        {
            List<User> users = _context.Users.ToList();
            return View("Admin_users", users);
        }

        public IActionResult Orders()
        {
            List<Order> orders = _context.Orders.Include(o => o.User).ToList();
            return View("Admin_Orders", orders);
        }

        // User CRUD Operations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(user.Name))
                ModelState.AddModelError("Name", "Name is required");
            else
                user.Name = user.Name.Trim();

            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("Email", "Email is required");
            else
                user.Email = user.Email.Trim();

            if (string.IsNullOrWhiteSpace(user.Password))
                ModelState.AddModelError("Password", "Password is required");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required");
            else
                user.PhoneNumber = user.PhoneNumber.Trim();

            if (string.IsNullOrWhiteSpace(user.Address))
                ModelState.AddModelError("Address", "Address is required");
            else
                user.Address = user.Address.Trim();

            if (string.IsNullOrWhiteSpace(user.Gender))
                ModelState.AddModelError("Gender", "Gender is required");

            // Ensure UserRole is valid
            if (user.UserRole < 0 || user.UserRole > 1)
                ModelState.AddModelError("UserRole", "Invalid role selected");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Users");
            }

            try
            {
                // Set default values
                user.RegistrationDate = DateTime.UtcNow;
                
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["Success"] = "User created successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create user: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUser(User user)
        {
            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(user.Name))
                ModelState.AddModelError("Name", "Name is required");
            else
                user.Name = user.Name.Trim();

            if (string.IsNullOrWhiteSpace(user.Email))
                ModelState.AddModelError("Email", "Email is required");
            else
                user.Email = user.Email.Trim();

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required");
            else
                user.PhoneNumber = user.PhoneNumber.Trim();

            if (string.IsNullOrWhiteSpace(user.Address))
                ModelState.AddModelError("Address", "Address is required");
            else
                user.Address = user.Address.Trim();

            if (string.IsNullOrWhiteSpace(user.Gender))
                ModelState.AddModelError("Gender", "Gender is required");

            // Ensure UserRole is valid
            if (user.UserRole < 0 || user.UserRole > 1)
                ModelState.AddModelError("UserRole", "Invalid role selected");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Users");
            }

            try
            {
                var existingUser = _context.Users.Find(user.Id);
                if (existingUser != null)
                {
                    existingUser.Name = user.Name;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    existingUser.Address = user.Address;
                    existingUser.UserRole = user.UserRole;
                    existingUser.Gender = user.Gender;
                    
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = user.Password;
                    }

                    _context.SaveChanges();
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
                else
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update user: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete user: {ex.Message}";
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            return Json(user);
        }

        // Product CRUD Operations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product product)
        {
            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(product.Name))
                ModelState.AddModelError("Name", "Product name is required");
            else
                product.Name = product.Name.Trim();

            if (string.IsNullOrWhiteSpace(product.Description))
                ModelState.AddModelError("Description", "Product description is required");
            else
                product.Description = product.Description.Trim();

            if (product.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than 0");

            if (product.CategoryId <= 0)
                ModelState.AddModelError("CategoryId", "Please select a valid category");

            if (product.StockS < 0)
                ModelState.AddModelError("StockS", "Small stock cannot be negative");

            if (product.StockM < 0)
                ModelState.AddModelError("StockM", "Medium stock cannot be negative");

            if (product.StockL < 0)
                ModelState.AddModelError("StockL", "Large stock cannot be negative");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Products");
            }

            try
            {
                // Set default values
                product.CreatedDate = DateTime.UtcNow;
                
                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to create product: {ex.Message}";
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProduct(Product product)
        {
            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(product.Name))
                ModelState.AddModelError("Name", "Product name is required");
            else
                product.Name = product.Name.Trim();

            if (string.IsNullOrWhiteSpace(product.Description))
                ModelState.AddModelError("Description", "Product description is required");
            else
                product.Description = product.Description.Trim();

            if (product.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than 0");

            if (product.CategoryId <= 0)
                ModelState.AddModelError("CategoryId", "Please select a valid category");

            if (product.StockS < 0)
                ModelState.AddModelError("StockS", "Small stock cannot be negative");

            if (product.StockM < 0)
                ModelState.AddModelError("StockM", "Medium stock cannot be negative");

            if (product.StockL < 0)
                ModelState.AddModelError("StockL", "Large stock cannot be negative");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Products");
            }

            try
            {
                var existingProduct = _context.Products.Find(product.Id);
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.StockS = product.StockS;
                    existingProduct.StockM = product.StockM;
                    existingProduct.StockL = product.StockL;
                    existingProduct.CategoryId = product.CategoryId;

                    _context.SaveChanges();
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update product: {ex.Message}";
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product != null)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to delete product: {ex.Message}";
            }
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            return Json(product);
        }

        // Test action to bypass model binding
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestCreateUser(string Name, string Email, string Password, string PhoneNumber, string Address, int UserRole, string Gender)
        {
            try
            {
                var user = new User
                {
                    Name = Name?.Trim(),
                    Email = Email?.Trim(),
                    Password = Password,
                    PhoneNumber = PhoneNumber?.Trim(),
                    Address = Address?.Trim(),
                    UserRole = UserRole,
                    Gender = Gender,
                    RegistrationDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["Success"] = "User created successfully via test method!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Test method failed: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        // Test action for products
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestCreateProduct(string Name, string Description, decimal Price, string ImageUrl, int CategoryId, int StockS, int StockM, int StockL)
        {
            try
            {
                var product = new Product
                {
                    Name = Name?.Trim(),
                    Description = Description?.Trim(),
                    Price = Price,
                    ImageUrl = ImageUrl?.Trim(),
                    CategoryId = CategoryId,
                    StockS = StockS,
                    StockM = StockM,
                    StockL = StockL,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Products.Add(product);
                _context.SaveChanges();
                TempData["Success"] = "Product created successfully via test method!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Test method failed: {ex.Message}";
                return RedirectToAction("Products");
            }
        }
    }
}