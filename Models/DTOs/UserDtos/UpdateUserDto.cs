using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.DTOs.UserDtos
{
    public class UpdateUserDto
    {
        [MaxLength(50)]
        [Column("first_name")]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        [Column("last_name")]
        public string? LastName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        [Column("email")]
        public string? Email { get; set; }

        [MaxLength(200)]
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
    }
}
