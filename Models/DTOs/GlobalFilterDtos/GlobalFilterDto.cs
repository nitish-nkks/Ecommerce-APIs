namespace Ecommerce_APIs.Models.DTOs.GlobalFilterDtos
{
    public class GlobalFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "CreatedAt"; // default sort field
        public bool Descending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsActive { get; set; } = true;
        public string? Format { get; set; } = "json"; // Optional: "json" or "excel"
    }

}
