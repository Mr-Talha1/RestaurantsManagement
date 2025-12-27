using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class Customers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CustomerId { get; set; }

        [Required]
        public long MerchantId { get; set; }

        public string? CustomerName { get; set; }

        [Required]
        public string CustomerPhone { get; set; } = null!;

        public string? DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
