using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.InternalUserDtos
{
    public class UpdateInternalUserDto
    {
        public string? UserName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public required string? PasswordHash { get; set; }
        public string? Role { get; set; }

    }
}
