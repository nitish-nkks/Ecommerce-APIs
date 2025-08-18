namespace Ecommerce_APIs.Models.DTOs.CatagoriesDtos
{
    public class AddCategoryDto
    {
        public string Name { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

}
