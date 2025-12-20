using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class OrderItems
    {
        [Key]
        public int Id { get; set; }
        public long OrderId { get; set; }
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal? AmountAfterDiscount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
