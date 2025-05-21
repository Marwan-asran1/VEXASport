using Microsoft.AspNetCore.Mvc;
using System.Linq;
using VEXA.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AspNetCoreGeneratedDocument;

namespace VEXA.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: All Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View("~/Views/Admin/Admin_users");
        }

        // GET: Get User by ID (for modal)
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return Json(new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                address = user.Address,
                gender = user.Gender,
                userRole = user.UserRole.ToString()
            });
        }

        // POST: Edit User (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("Id,Name,Email,PhoneNumber,Address,Gender,UserRole")] User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return Json(new { success = false, message = "User ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed.", errors });
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.Address = updatedUser.Address;
            existingUser.Gender = updatedUser.Gender;
            existingUser.UserRole = updatedUser.UserRole;

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "User updated successfully.",
                    user = new
                    {
                        id = existingUser.Id,
                        name = existingUser.Name,
                        email = existingUser.Email,
                        phoneNumber = existingUser.PhoneNumber,
                        address = existingUser.Address,
                        gender = existingUser.Gender,
                        userRole = existingUser.UserRole.ToString()
                    }
                });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to update user." });
            }
        }

        // POST: Delete User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return View("~/Views/Admin/Amin_users.cshtml");
        }
    }
}
