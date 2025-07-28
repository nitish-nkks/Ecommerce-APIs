namespace Ecommerce_APIs.Models.DTOs.CartIteamsDtos
{
    public class CreateCartItemDto
    {
        public string? GuestId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
