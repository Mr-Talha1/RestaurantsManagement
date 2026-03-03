using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.DTO
{
    public class EditOrderRequest
    {
        [Required]
        public long OrderId { get; set; }

        // Customer Information
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? DeliveryAddress { get; set; }

        // Payment
        [Required]
        public string PaymentType { get; set; }

        // Discounts
        public string? OrderDiscountType { get; set; } // "percentage" or "flat"
        public decimal? OrderDiscountValue { get; set; }

        // TAX FIELDS - ADD THESE
        public string? TaxType { get; set; } // "percentage" or "flat" - optional
        public decimal? TaxValue { get; set; } // 15 or 5 - optional

        // Items
        [Required]
        [MinLength(1)]
        public List<EditOrderItemDto> Items { get; set; }
    }

    public class EditOrderItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Qty { get; set; }

        public string? DiscountType { get; set; } // "percentage" or "flat"
        public decimal? DiscountValue { get; set; }
    }
}
