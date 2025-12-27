namespace BIPL_RAASTP2M.DTO
{
    public class AddOrderRequest
    {
        public string OrderType { get; set; }       // Dining / Takeaway / Delivery
        public string PaymentType { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? OrderDiscountType { get; set; }
        public decimal? OrderDiscountValue { get; set; }
        public int? TableId { get; set; }           // Only for Dining
        public long OrderNumber { get; set; }

        //Customer details
        public string? CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string? DeliveryAddress { get; set; }
        public List<OrderItemRequest> Items { get; set; }

    }
}
