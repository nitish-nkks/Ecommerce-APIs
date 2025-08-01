namespace Ecommerce_APIs.Models.DTOs.CatagoriesDtos
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
    }

}
