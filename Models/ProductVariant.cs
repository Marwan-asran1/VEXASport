using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public ClothingSize Size { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}


