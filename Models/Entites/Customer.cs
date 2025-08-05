using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{

    [Table("customers")]
    public class Customer
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

        [MaxLength(20)]
        [Required]
        [Column("phone_number")]
        public required string PhoneNumber { get; set; }
        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();

        [Required]
        [MaxLength(200)]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();


    }

}
