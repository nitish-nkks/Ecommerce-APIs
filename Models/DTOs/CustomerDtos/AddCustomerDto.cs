using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Models.DTOs.UserDtos
{
    public class AddCustomerDto
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
        [MaxLength(200)]
        [Column("password_hash")]
        public required string PasswordHash { get; set; }

        [MaxLength(20)]
        public required string PhoneNumber { get; set; }
    }
}
