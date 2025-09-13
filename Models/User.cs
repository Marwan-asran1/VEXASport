using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class User
    {
        public User()
        {
            Name = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            PhoneNumber = string.Empty;
            Address = string.Empty;
            Gender = CustomerGender.Unspecified;
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
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [StringLength(100)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(0, 1, ErrorMessage = "Invalid role selected")]
        public int UserRole { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public CustomerGender Gender { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}
