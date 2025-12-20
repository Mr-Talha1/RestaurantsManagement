namespace BIPL_RAASTP2M.Models
{
    public class Products
    {

        public int Id { get; set; }
        public long MerchantId { get; set; }
        public int? CategoryId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ImagePath { get; set; }
        public string? ImagePublicId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
