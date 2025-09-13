using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace VEXA.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(AppDbContext context, ILogger<UserController> logger, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Invalid credentials";
                return View();
            }
            // Verify hashed password; support legacy plaintext migration on successful match
            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (verification == PasswordVerificationResult.Failed)
            {
                // Legacy path: user.Password stores plaintext
                if (user.Password == password)
                {
                    user.Password = _passwordHasher.HashPassword(user, password);
                    _context.SaveChanges();
                }
                else
                {
                    TempData["Error"] = "Invalid credentials";
                    return View();
                }
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRole == 1 ? "Admin" : "Customer")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult Register()
        {
            var hasAdmin = _context.Users.Any(u => u.UserRole == 1);
            ViewBag.HasAdmin = hasAdmin;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user, int accountType)
        {
            if (string.IsNullOrWhiteSpace(user.Name)) ModelState.AddModelError("Name", "Name is required");
            if (string.IsNullOrWhiteSpace(user.Email)) ModelState.AddModelError("Email", "Email is required");
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6) ModelState.AddModelError("Password", "Password must be at least 6 characters");
            if (string.IsNullOrWhiteSpace(user.PhoneNumber)) ModelState.AddModelError("PhoneNumber", "Phone is required");
            if (string.IsNullOrWhiteSpace(user.Address)) ModelState.AddModelError("Address", "Address is required");
            // Gender validation is handled by the enum default value

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already registered");
            }

            // Check if admin already exists and prevent creating another admin
            if (accountType == 1 && _context.Users.Any(u => u.UserRole == 1))
            {
                ModelState.AddModelError("", "Admin account already exists. Only one admin is allowed.");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.RegistrationDate = DateTime.UtcNow;
            user.UserRole = accountType; // Use the selected account type
            // Hash password before saving
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            var roleName = accountType == 1 ? "Admin" : "Customer";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(User user)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the form and try again.";
                return View("Profile", user);
            }

            try
            {
                var existingUser = _context.Users.Find(user.Id);
                if (existingUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Update only allowed fields
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;
                existingUser.Gender = user.Gender;

                // Only update password if provided
                if (!string.IsNullOrEmpty(user.Password) && user.Password.Length >= 6)
                {
                    existingUser.Password = _passwordHasher.HashPassword(existingUser, user.Password);
                }

                _context.SaveChanges();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile", new { id = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
                TempData["Error"] = "An error occurred while updating your profile. Please try again.";
                return View("Profile", user);
            }
        }

        public IActionResult ChangePassword(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(int id, string currentPassword, string newPassword, string confirmPassword)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.Password, currentPassword);
            if (verification == PasswordVerificationResult.Failed && user.Password != currentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return View(user);
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return View(user);
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters long.";
                return View(user);
            }

            try
            {
                user.Password = _passwordHasher.HashPassword(user, newPassword);
                _context.SaveChanges();
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Profile", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                TempData["Error"] = "An error occurred while changing password. Please try again.";
                return View(user);
            }
        }

        // DebugUser endpoint removed for security
    }
}
