using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.Models
{
    public class Orders
    {
        [Key]
        public long Id { get; set; }
        public long MerchantId { get; set; }
        public string? UserId { get; set; }
        public long OrderNumber { get; set; }
        public long InvoiceId { get; set; }
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
        public long? CustomerId { get; set; }
        public bool IsRefunded { get; set; }
        public string? RefundedBy { get; set; }
        public DateTime? RefundedAt { get; set; }
        //---------- tax
        public string? TaxType { get; set; }         // "percentage" or "flat" - now optional
        public decimal? TaxValue { get; set; }        // 15 or 5 - now optional
        public decimal? TaxAmount { get; set; }

        // EDIT TRACKING FIELDS
        public bool IsEdited { get; set; } = false;
        public string? EditedBy { get; set; }
        public DateTime? EditedAt { get; set; }
        public int EditCount { get; set; } = 0;

        // ADD THIS NAVIGATION PROPERTY (Fixes error CS1061)
        public ICollection<OrderItems> OrderItems { get; set; }
    }
}
