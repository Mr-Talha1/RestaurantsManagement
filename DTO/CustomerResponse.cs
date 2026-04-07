namespace TBAppBackend.DTO
{
    public class CustomerResponse
    {

        public long CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? DeliveryAddress { get; set; }
    }
}
