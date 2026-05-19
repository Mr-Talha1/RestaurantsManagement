using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.DTO
{
    public class BranchDto
    {
        [Required]
        public string? BranchName { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        public int? CityID { get; set; }
        [Required]
        public bool? Active { get; set; }

    }
}
