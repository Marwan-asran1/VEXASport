using VEXA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEXA.Models
{
    public enum Size
    {
        S, M, L
    }

    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Size Size { get; set; } = Size.S;

        // Category relationship
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? MainCategory 
        { 
            get 
            {
                if (Category?.ParentCategory == null) return null;
                return Category.ParentCategory.Name;
            }
        }

        public string? SubCategory 
        { 
            get 
            {
                if (Category == null) return null;
                return Category.Name;
            }
        }

        public ICollection<Review>? Reviews { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
