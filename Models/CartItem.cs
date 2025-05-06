using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string ImageFileName { get; set; }

        //public string Size { get; set; }

        public int Quantity { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public decimal TotalPrice => Product?.Price * Quantity ?? 0;
    }
} 