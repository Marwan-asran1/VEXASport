using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class User
    {
        public User()
        {
            RegistrationDate = DateTime.UtcNow;
        }

        public enum Role
        {
            Customer = 0,
            Admin = 1
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int UserRole { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}
