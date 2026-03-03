using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.DTO
{
    public class ReportRequestDto
    {
        [Required]
        public string FromDate { get; set; }

        [Required]
        public string ToDate { get; set; }
    }
}
