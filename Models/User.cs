using VEXA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEXA.Models
{
    public class User
    {
        //public enum Gender
        //{
        //    Male,Female
        //}

        public enum Role
        {
            Customer,
            Admin
        }

        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public Role UserRole { get; set; }
        //public Gender UserGender { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
