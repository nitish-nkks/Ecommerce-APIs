namespace Ecommerce_APIs.Models.DTOs.ContactMessageDtos
{
    public class ContactMessageCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Message { get; set; } = string.Empty;
    }

}
