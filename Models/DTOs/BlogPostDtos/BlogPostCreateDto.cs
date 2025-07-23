namespace Ecommerce_APIs.Models.DTOs.BlogPostDtos
{
    public class BlogPostCreateDto
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string Status { get; set; } = "draft";
        public int? AuthorId { get; set; }
    }
}
