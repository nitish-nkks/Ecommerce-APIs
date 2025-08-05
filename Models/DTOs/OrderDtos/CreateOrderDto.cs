using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerAddressId { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }

}
