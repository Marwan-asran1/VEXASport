using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VEXA.Models;
using System.Security.Claims;

namespace VEXA.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "User");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateField(string field, string value)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Validate and update the specified field
                switch (field.ToLower())
                {
                    case "name":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Name cannot be empty" });
                        }
                        user.Name = value.Trim();
                        break;

                    case "phone":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Phone number cannot be empty" });
                        }
                        if (value.Length != 11)
                        {
                            return Json(new { success = false, message = "Phone number must be exactly 11 digits" });
                        }
                        user.PhoneNumber = value.Trim();
                        break;

                    case "address":
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            return Json(new { success = false, message = "Address cannot be empty" });
                        }
                        user.Address = value.Trim();
                        break;

                    case "email":
                        return Json(new { success = false, message = "Email cannot be changed" });

                    default:
                        return Json(new { success = false, message = "Invalid field" });
                }

                _context.SaveChanges();
                return Json(new { success = true, message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating profile: " + ex.Message });
            }
        }
    }
}
