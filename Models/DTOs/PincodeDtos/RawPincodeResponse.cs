namespace Ecommerce_APIs.Models.DTOs.PincodeDtos
{
    public class RawPincodeResponse
    {
        public string Message { get; set; } = null!;
        public string Status { get; set; } = null!;
        public List<RawPostOffice> PostOffice { get; set; } = new();
    }

    public class RawPostOffice
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string BranchType { get; set; } = null!;
        public string DeliveryStatus { get; set; } = null!;
        public string Circle { get; set; } = null!;
        public string District { get; set; } = null!;
        public string Division { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string Block { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Pincode { get; set; } = null!;
    }
}
