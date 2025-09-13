using System.ComponentModel.DataAnnotations;

namespace VEXA.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int UserId { get; set; }

        public Product? Product { get; set; }
        public User? User { get; set; }
    }
}

