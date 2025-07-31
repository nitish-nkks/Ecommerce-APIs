using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_APIs.Models.Entites
{
    public enum Role
    {
        Admin,
        Staff,
        Manager,
        Support,
        Sales,
    }

    [Table("internal_users")]
    public class InternalUser
    {
        [Key]
        [Column("id")]
        public int Id { get; set; } 

        [MaxLength(50)]
        [Column("employee_id")]
        public string? EmployeeId { get; set; }

        [MaxLength(100)]
        [Column("user_name")]
        public required string UserName { get; set; }

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
        public Role Role { get; set; }

        [MaxLength(50)]
        [Column("designation")]
        public string? Designation { get; set; }

        [MaxLength(100)]
        [Column("department")]
        public string? Department { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }
    }

}
