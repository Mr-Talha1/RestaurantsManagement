namespace BIPL_RAASTP2M.DTO
{
    public class AddProductRequest
    {
        public int? Id { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
