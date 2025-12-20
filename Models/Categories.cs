using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class Categories
    {
        [Key]
        public int Id { get; set; }
        public long MerchantId { get; set; }
        public string CategoryName { get; set; } = null!;
        public bool IsDeleted { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
