using System;
using System.Collections.Generic;

namespace VEXA.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int StockS { get; set; }
        public int StockM { get; set; }
        public int StockL { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Convenience properties
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
        //public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
