using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce_APIs.Models.Entites
{
    [Table("customer_addresses")]
    public class CustomerAddress
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [MaxLength(250)]
        [Column("address_line")]
        public string AddressLine { get; set; }

        [MaxLength(100)]
        [Column("city")]
        public string City { get; set; }

        [MaxLength(100)]
        [Column("state")]
        public string State { get; set; }

        [MaxLength(10)]
        [Column("zip_code")]
        public string ZipCode { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        public Customer Customer { get; set; }
    }

}
