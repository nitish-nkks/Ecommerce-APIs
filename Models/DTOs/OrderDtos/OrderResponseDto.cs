namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; }
    }

}
