namespace BIPL_RAASTP2M.DTO
{
    public class OrderHistoryResponse
    {
        public long Id { get; set; }
        public long OrderNumber { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public string? TableName { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }
    public class OrderItemResponse
    {
        public string ProductName { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
