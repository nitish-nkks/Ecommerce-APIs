using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Models.DTOs.BlogPostDtos
{
    public class BlogPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public int? CreatedBy { get; set; }
    }
}