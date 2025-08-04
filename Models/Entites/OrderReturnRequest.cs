using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public enum ReturnStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class OrderReturnRequest
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Reason { get; set; }
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        public string? AdminRemarks { get; set; }

        public int RequestedBy { get; set; }
        public string RequestedByUserType { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? ActionedAt { get; set; }

        public int? ActionedBy { get; set; }
        public string? ActionedByUserType { get; set; }
    }

}
