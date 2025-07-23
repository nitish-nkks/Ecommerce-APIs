namespace Ecommerce_APIs.Models.DTOs.StaticPageDtos
{
    public class StaticPageCreateDto
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int? CreatedBy { get; set; }
    }
}
