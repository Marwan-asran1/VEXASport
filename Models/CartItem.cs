using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Size is required")]
        public ClothingSize Size { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "User is required")]
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