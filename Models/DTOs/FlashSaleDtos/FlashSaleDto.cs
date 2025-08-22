using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.DTOs.FlashSaleDtos
{
    public class FlashSaleDto
    {
        public int ProductId { get; set; }
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "datetime(3)")]
        public DateTime? EndDate { get; set; }
        public DayOfWeek? SaleDay { get; set; }
        public TimeSpan? StartTime { get; set; }   
        public TimeSpan? EndTime { get; set; }

    }

}
