using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? Size { get; set; }

        public int Quantity { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal TotalPrice
        {
            get
            {
                if (Product == null)
                    return 0;
                return Product.Price * Quantity;
            }
        }
    }
} 