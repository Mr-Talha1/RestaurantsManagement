using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class WebsiteConfig
    {
        [Key]
        public long Id { get; set; }

        // Foreign key to Merchants table
        public long MerchantId { get; set; }

        // Website specific fields
        public string Subdomain { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroDescription { get; set; }
        public string? WorkingHours { get; set; } // JSON string
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactAddress { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("MerchantId")]
        public virtual Merchants Merchant { get; set; } = null!;
    }
}
