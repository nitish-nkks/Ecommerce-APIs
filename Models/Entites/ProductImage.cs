namespace Ecommerce_APIs.Models.Entites
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
