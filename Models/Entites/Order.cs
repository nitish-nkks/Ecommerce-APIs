using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public enum OrderStatus
    {
        Order_Placed,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Return_Requested,
        Returned
    }
    public class Order
    {
        public int Id { get; set; }

        [ForeignKey("InternalUser")]
        public int? InternalUserId { get; set; }
        public InternalUser? InternalUser { get; set; }

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string? CreatedByUserType { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Order_Placed;
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CustomerAddress")]
        public int CustomerAddressId { get; set; }
        public CustomerAddress CustomerAddress { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

}
