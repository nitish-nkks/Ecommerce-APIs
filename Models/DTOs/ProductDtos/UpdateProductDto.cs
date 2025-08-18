using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.ProductDtos
{
    public class UpdateProductDto
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
        public int? MinOrderQuantity { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNewProduct { get; set; }
        public bool IsBestSeller { get; set; }
        public List<string>? ProductImages { get; set; }
    }
}
