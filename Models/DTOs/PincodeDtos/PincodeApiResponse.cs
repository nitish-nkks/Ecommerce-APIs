namespace Ecommerce_APIs.Models.DTOs.PincodeDtos
{
    public class PincodeApiResponse
    {
        public string Pincode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool Deliverable { get; set; }
        public List<DeliveryOffice> DeliveryOffices { get; set; } = new();
    }


public class DeliveryOffice
{
    public string Name { get; set; } = null!;
    public string BranchType { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
}
}