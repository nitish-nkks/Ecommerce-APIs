namespace Ecommerce_APIs.Models.DTOs.OrderTrackingDtos
{
    public class CreateOrderTrackingDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? TrackingId { get; set; }
        public string? Remarks { get; set; }
    }
}
