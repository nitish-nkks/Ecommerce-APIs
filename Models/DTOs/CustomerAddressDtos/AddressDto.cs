using System.ComponentModel.DataAnnotations;

namespace Ecommerce_APIs.Models.DTOs.CustomerAddressDtos
{
    public class AddressDto
    {
        [Required]
        [MaxLength(250)]
        public string AddressLine { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        public string State { get; set; }

        [Required]
        [MaxLength(10)]
        public string ZipCode { get; set; }
    }
}
