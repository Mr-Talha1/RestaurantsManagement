namespace BIPL_RAASTP2M.Models
{
    public class DiningTables
    {
        public int Id { get; set; }
        public long MerchantId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
