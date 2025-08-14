using Ecommerce_APIs.Models.Entites;
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int? ParentCategoryId { get; set; } = null;
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }

    public int CreatedById { get; set; }
    public string CreatedByType { get; set; } = "InternalUser";
    public InternalUser CreatedBy { get; set; }
    public int? UpdatedById { get; set; }
    public InternalUser? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product>? Products { get; set; }
}
