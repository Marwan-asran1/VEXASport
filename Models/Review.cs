using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEXA.Models;

namespace VEXA
{

    public class Review
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        public int ProductId { get; set; }
        public int UserId { get; set; }


        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}

