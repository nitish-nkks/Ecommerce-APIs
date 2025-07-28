namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
    }

}
