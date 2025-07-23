namespace Ecommerce_APIs.Models.DTOs.OrderTrackingDtos
    {
        public class OrderTrackingDto
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int UserId { get; set; }
            public string? TrackingId { get; set; }
            public string CurrentStatus { get; set; }
            public string? Remarks { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }