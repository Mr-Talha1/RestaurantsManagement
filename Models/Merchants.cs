using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class Merchants
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? CnicFrontPath { get; set; }
        public string? CnicBackPath { get; set; }
        public int? OrderLimit { get; set; }
        public string? Website { get; set; }
        public string? MobileNumber { get; set; }
        public string? LogoPath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
