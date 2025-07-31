using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce_APIs.Models.Entities;

namespace Ecommerce_APIs.Models.Entites
{
    public class CartItem
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
        public string? GuestId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } 

        public int Quantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

}
