namespace TBAppBackend.DTO
{
    public class ReportResponseDto
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public ReportDataDto Data { get; set; }
    }

    public class ReportDataDto
    {
        public KpiDto Kpi { get; set; }
        public List<ProductStatDto> ProductStats { get; set; }
        public TimeDataDto TimeData { get; set; }
        public TaxSummaryDto TaxSummary { get; set; }
        public DiscountSummaryDto DiscountSummary { get; set; }
        public List<PaymentMethodDto> PaymentMethodStats { get; set; }
        public OrderStatsDto OrderStats { get; set; }
    }

    public class KpiDto
    {
        public int TotalOrders { get; set; }
        public int TotalItemsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalOriginalRevenue { get; set; }
        public decimal TotalDiscount { get; set; }
    }

    public class ProductStatDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal OriginalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public int? ItemsDiscounted { get; set; }
    }

    public class TaxSummaryDto
    {
        public decimal TotalTaxCollected { get; set; }
        public List<TaxByPaymentMethodDto> TaxByPaymentMethod { get; set; }
    }

    public class TaxByPaymentMethodDto
    {
        public string PaymentMethod { get; set; }
        public decimal TaxAmount { get; set; }
    }

    public class DiscountSummaryDto
    {
        public decimal TotalDiscount { get; set; }
        public List<ProductDiscountDto> ProductDiscounts { get; set; }
    }

    public class ProductDiscountDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal DiscountAmount { get; set; }
        public int ItemsDiscounted { get; set; }
        public decimal? AverageDiscountRate { get; set; }
    }

    public class PaymentMethodDto
    {
        public string Method { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int RefundedOrders { get; set; }
        public decimal RefundedAmount { get; set; }
        public string BestSellingTime { get; set; }
    }

    public class TimeDataDto
    {
        public string Type { get; set; }
        public List<TimePointDto> Points { get; set; }
    }

    public class TimePointDto
    {
        public string Label { get; set; }
        public DateTime? Date { get; set; }
        public int? Hour { get; set; }
        public decimal Value { get; set; }
    }

}
