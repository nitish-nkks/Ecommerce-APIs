namespace Ecommerce_APIs.Models.DTOs.OrderDtos
{
    public class ReturnStatusUpdateDto
    {
        public required string Status { get; set; }
        public string? AdminRemarks { get; set; }
    }
}
