namespace Ecommerce_APIs.Models.DTOs.ProductDtos
{
        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public decimal DiscountPercentage { get; set; }
            public bool IsFeatured { get; set; }
            public bool IsNewProduct { get; set; }
            public bool IsBestSeller { get; set; }
            public int StockQuantity { get; set; }
            public int MinOrderQuantity { get; set; }
            public int MaxOrderQuantity { get; set; }
            public string? Image { get; set; }
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }
            public DateTime CreatedAt { get; set; }
        }
}
