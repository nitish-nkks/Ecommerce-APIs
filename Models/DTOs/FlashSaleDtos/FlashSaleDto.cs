namespace Ecommerce_APIs.Models.DTOs.FlashSaleDtos
{
    public class FlashSaleDto
    {
        public int ProductId { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
