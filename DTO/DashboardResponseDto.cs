namespace TBAppBackend.DTO
{
    public class DashboardResponseDto
    {
        public DashboardSummaryDto Summary { get; set; }
        public List<RevenueTrendDto> RevenueTrend { get; set; }
        //public List<CategoryRevenueDto> CategoryRevenue { get; set; }
        public List<PaymentMethodDashboardDto> PaymentMethods { get; set; }
        public DashboardTimeDataDto TimeData { get; set; }
        //public List<HourlyOrderDto> HourlyOrders { get; set; }
        public BranchPerformanceDto BranchPerformance { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; }
        public List<TopProductDto> TopProducts { get; set; }
    }

    public class DashboardSummaryDto
    {
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal AvgOrder { get; set; }
        public int Customers { get; set; }
        public int ActiveStaff { get; set; }
        public int TotalStaff { get; set; }
    }

    public class RevenueTrendDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategoryRevenueDto
    {
        public string CategoryName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }


    public class PaymentMethodDashboardDto
    {
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class HourlyOrderDto
    {
        public int Hour { get; set; }
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
    public class BranchPerformanceDto
    {
        public string BranchName { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
    public class RecentOrderDto
    {
        public long Id { get; set; }
        public long OrderNumber { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentType { get; set; }
    }
    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public decimal OriginalRevenue { get; set; }
    }
    //  day graph 

    public class DashboardTimeDataDto
    {
        public string Type { get; set; } // Hourly / Daily
        public List<DashboardTimeItemDto> Data { get; set; }
    }

    public class DashboardTimeItemDto
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
