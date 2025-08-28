namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }

}
