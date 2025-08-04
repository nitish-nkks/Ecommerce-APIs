using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public OrderStatus Status { get; set; }

        public string? ChangedByUserType { get; set; }
        public int? ChangedByUserId { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.Now;

        public string? Remarks { get; set; } 
    }

}
