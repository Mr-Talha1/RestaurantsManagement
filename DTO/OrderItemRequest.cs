namespace BIPL_RAASTP2M.DTO
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public string? DiscountType { get; set; } // "Percentage" or "Flat"
        public decimal? DiscountValue { get; set; } // 8 or 100
    }
}
