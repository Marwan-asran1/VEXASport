using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace VEXA.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        //public string? Size { get; set; } = "S,M,L";

        
        //public List<string> SizeList =>
    public int StockS { get; set; }
    public int StockM { get; set; }
    public int StockL { get; set; }
        //    Size?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string> { "S", "M", "L" };

        public int CategoryId { get; set; }
        public Category? Category { get; set; }


<<<<<<< HEAD
        public ICollection<Review>? Reviews { get; set; }
        //public ICollection<OrderItem>? OrderItems { get; set; }
    }
=======
    public ICollection<OrderItem>? OrderItems { get; set; }
>>>>>>> admin
}
}