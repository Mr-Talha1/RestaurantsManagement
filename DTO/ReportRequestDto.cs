using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.DTO
{
    public class ReportRequestDto
    {
        [Required]
        public string FromDate { get; set; }

        [Required]
        public string ToDate { get; set; }
    }
}
