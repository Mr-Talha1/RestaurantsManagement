namespace BIPL_RAASTP2M.DTO
{
    public class UpdateWebsiteConfigDto
    {
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? HeroTitle { get; set; }
        public string? HeroDescription { get; set; }
        public List<UpdateWorkingHoursDto>? WorkingHours { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactAddress { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateWorkingHoursDto
    {
        public string Day { get; set; } = null!;
        public string Open { get; set; } = null!;
        public string Close { get; set; } = null!;
        public bool Closed { get; set; }
    }
}
