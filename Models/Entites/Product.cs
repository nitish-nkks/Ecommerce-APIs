using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? CategoryBy { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<CartItem> CartItems { get; set; }


    }
}
