using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    public enum UserRole
    {
        Customer, // 0 for coustomer and 1 for admin
        Admin
    }

    [Table("users")]
    public class Users
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column("first_name")]
        public required string FirstName { get; set; }

        [MaxLength(50)]
        [Column("last_name")]
        public required string LastName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        [Column("email")]
        public required string Email { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Required]
        [Column("role")]
        public UserRole Role { get; set; } = UserRole.Customer;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();


    }

}
