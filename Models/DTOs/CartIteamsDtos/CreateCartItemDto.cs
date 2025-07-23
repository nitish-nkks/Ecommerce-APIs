namespace Ecommerce_APIs.Models.DTOs.CartIteamsDtos
{
    public class CreateCartItemDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
