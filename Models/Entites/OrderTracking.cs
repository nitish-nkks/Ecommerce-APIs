namespace Ecommerce_APIs.Models.Entites
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Ecommerce_APIs.Models.Entities
    {
        public enum TrackingStatus
        {
            Pending,
            Confirmed,
            Shipped,
            InTransit,
            Delivered,
            Cancelled
        }

        public class OrderTracking
        {
            public int Id { get; set; }

            [ForeignKey("Order")]
            public int OrderId { get; set; }

            [ForeignKey("User")]
            public int UserId { get; set; }

            [MaxLength(100)]
            public string? TrackingId { get; set; }

            public TrackingStatus CurrentStatus { get; set; } = TrackingStatus.Pending;

            public string? Remarks { get; set; }

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }

            public Order Order { get; set; }
            public Users User { get; set; }
        }
    }

}
