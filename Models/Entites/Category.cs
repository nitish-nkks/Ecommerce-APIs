using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Models.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? ParentCategoryId { get; set; } = null;
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }

    public int CreatedById { get; set; }
    public Users CreatedBy { get; set; }
    public int? UpdatedById { get; set; }
    public Users? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Product>? Products { get; set; }
}
