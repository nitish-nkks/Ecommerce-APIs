namespace Ecommerce_APIs.Models.DTOs.CatagoriesDtos
{
    public class AddCategoryDto
    {
        public string Name { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public IFormFile? Image { get; set; }
        public string? Description { get; set; }
        public IFormFile? Icon { get; set; }
    }

}
