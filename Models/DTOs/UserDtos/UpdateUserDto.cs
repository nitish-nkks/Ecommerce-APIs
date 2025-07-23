using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.DTOs.UserDtos
{
    public class UpdateUserDto
    {
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
        [MaxLength(25)]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }
    }
}
