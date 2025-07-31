using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Models.DTOs.InternalUserDtos
{
    public class CreateInternalUserDto
    {
        public string? EmployeeId { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
        public required string PasswordHash { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
    }
}
