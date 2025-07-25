﻿namespace Ecommerce_APIs.Models.DTOs.CartIteamsDtos
{
    public class CartItemResponseDto
    {
        public int Id { get; set; }                  
        public int ProductId { get; set; }           
        public string ProductName { get; set; }    
        public string? ProductImageUrl { get; set; }  
        public decimal UnitPrice { get; set; } 
        public int Quantity { get; set; }         
        public decimal TotalPrice => Quantity * UnitPrice;
    }

}
