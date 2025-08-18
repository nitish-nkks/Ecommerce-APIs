using Newtonsoft.Json;

namespace Ecommerce_APIs.Models.DTOs.CatagoriesDtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
