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
            Customer,
            Admin
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public DateTime RegistrationDate { get; set; }

        public Role UserRole { get; set; }


        [Required]
        [RegularExpression("Male|Female", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public string Gender { get; set; }


        public ICollection<Order>? Orders { get; set; }

        public ICollection<Review>? Reviews { get; set; }
    }
}
