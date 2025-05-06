using VEXA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEXA.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        //public string? Size { get; set; } = "S,M,L";

        
        //public List<string> SizeList =>
        //    Size?.Split(',').Select(s => s.Trim()).ToList() ?? new List<string> { "S", "M", "L" };

        public int CategoryId { get; set; }
        public Category? Category { get; set; }


        public ICollection<Review>? Reviews { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
