using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public class CartItem
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public string? GuestId { get; set; }
        public string UserType { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } 

        public int Quantity { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }

}
