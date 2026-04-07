namespace TBAppBackend.DTO
{
    public class OrderHistoryResponse
    {
        public long Id { get; set; }
        public long OrderNumber { get; set; }
        public long InvoiceId { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public string? TableName { get; set; }
        public string? UserId { get; set; }
        public bool IsRefunded { get; set; }
        public string? RefundedBy { get; set; }
        public DateTime? RefundedAt { get; set; }

        // TAX FIELDS
        public string? TaxType { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? TaxAmount { get; set; }

        // PAYMENT TYPE
        public string? PaymentType { get; set; }

        // ORDER LEVEL DISCOUNT FIELDS (You already have these)
        public string? OrderDiscountType { get; set; }
        public decimal? OrderDiscountValue { get; set; }

        public List<OrderItemResponse> Items { get; set; }
        public CustomerResponse Customer { get; set; } // nullable

    }
    public class OrderItemResponse
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal TotalPrice { get; set; }

        // ADD THESE TWO DISCOUNT FIELDS
        public string? DiscountType { get; set; }      // "percentage" or "flat"
        public decimal? DiscountValue { get; set; }    // 10 or 100
    }
}
