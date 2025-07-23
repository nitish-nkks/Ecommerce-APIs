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

        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int? CreatedBy { get; set; }
    }
}
