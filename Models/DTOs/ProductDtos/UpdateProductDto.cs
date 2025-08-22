using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.ProductDtos
{
    public class UpdateProductDto
    {
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int? StockQuantity { get; set; }
        public int? MinOrderQuantity { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsNewProduct { get; set; }
        public bool? IsBestSeller { get; set; }
        public List<string>? ProductImages { get; set; }
    }
}
