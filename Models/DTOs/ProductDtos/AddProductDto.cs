using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.ProductDtos
{
    public class AddProductDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int MinOrderQuantity { get; set; }
        public List<string> ProductImages { get; set; } = new List<string>();
        public bool IsFeatured { get; set; }
        public bool IsNewProduct { get; set; }
        public bool IsBestSeller { get; set; }
    }
}
