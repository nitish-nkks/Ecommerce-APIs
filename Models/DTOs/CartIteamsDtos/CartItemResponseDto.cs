namespace Ecommerce_APIs.Models.DTOs.CartIteamsDtos
{
    public class CartItemResponseDto
    {           
        public int ProductId { get; set; }           
        public string Name { get; set; }    
        public string? Image { get; set; }  
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }         
        public decimal TotalPrice => Quantity * Price;
    }

}
