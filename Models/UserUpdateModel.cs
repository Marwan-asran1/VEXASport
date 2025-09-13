using System;
using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class UserUpdateModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Phone number must be exactly 11 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [Range(0, 1, ErrorMessage = "Invalid role selected")]
        public int UserRole { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public CustomerGender Gender { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
