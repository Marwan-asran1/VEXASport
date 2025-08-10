using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace VEXA.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
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
                    existingUser.Password = user.Password;
                }

                _context.SaveChanges();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile", new { id = user.Id });
            }
            catch (Exception ex)
            {
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

            if (user.Password != currentPassword)
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
                user.Password = newPassword;
                _context.SaveChanges();
                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Profile", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while changing password. Please try again.";
                return View(user);
            }
        }
    }
}
