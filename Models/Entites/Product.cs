using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountPercentage { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsNewProduct { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;
        public string? Image { get; set; }
        public int StockQuantity { get; set; }
        public int MinOrderQuantity { get; set; }
        public int MaxOrderQuantity => StockQuantity;
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<CartItem> CartItems { get; set; }


    }
}
