namespace Ecommerce_APIs.Models.DTOs.ProductImageDtos
{
    public class ProductImageCreateDto
    {
        public int ProductId { get; set; }
        public IFormFile Image { get; set; }
        public bool IsPrimary { get; set; }
    }

}
