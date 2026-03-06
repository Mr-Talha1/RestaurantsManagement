namespace BIPL_RAASTP2M.DTO
{
    public class WebsiteConfigResponseDto
    {
        public long Id { get; set; }
        public long MerchantId { get; set; }
        public string Subdomain { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroDescription { get; set; }
        public List<WorkingHoursResponseDto>? WorkingHours { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class WorkingHoursResponseDto
    {
        public string Day { get; set; } = null!;
        public string Open { get; set; } = null!;
        public string Close { get; set; } = null!;
        public bool Closed { get; set; }
    }
}

