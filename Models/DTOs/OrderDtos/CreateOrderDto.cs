namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        public int CustomerAddressId { get; set; }
        public string PaymentMethod { get; set; }
    }

}
