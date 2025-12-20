using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class Orders
    {
        [Key]
        public long Id { get; set; }
        public long MerchantId { get; set; }
        public int? UserId { get; set; }
        public long OrderNumber { get; set; }
        public string OrderType { get; set; }
        public int? TableId { get; set; }
        public string PaymentType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? GrossTotal { get; set; }
        public decimal? TotalDiscount { get; set; }
        public int ItemsCount { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? OrderDiscountType { get; set; }
        public decimal? OrderDiscountValue { get; set; }
        public decimal? OrderDiscountAmount { get; set; }
    }
}
