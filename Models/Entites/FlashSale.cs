using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce_APIs.Models.Entities;

namespace Ecommerce_APIs.Models.Entites
{
    public class FlashSale
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime EndDate { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
