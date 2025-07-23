namespace Ecommerce_APIs.Models.DTOs.CatagoriesDtos
{
    public class AddCategoryDto
    {
        public string Name { get; set; } = null!;
        public int? ParentCategoryId { get; set; }  
        public int CreatedById { get; set; }
    }

}
