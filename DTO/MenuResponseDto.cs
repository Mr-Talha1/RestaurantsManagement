namespace BIPL_RAASTP2M.DTO
{
    public class MenuResponseDto
    {
        public List<CategoryMenuDto> Categories { get; set; } = new();
    }
    public class CategoryMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        // Remove Description field
        public List<ProductMenuDto> Products { get; set; } = new();
    }

    public class ProductMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        // Remove Description field
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        // Remove IsAvailable field
        public int? CategoryId { get; set; }
    }
}
