using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.Models
{
    public class Branches
    {
        [Key]
        public int Id { get; set; }
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public int? CityID { get; set; }
        public bool? Active { get; set; }
        public long MerchantId { get; set; }
        public DateTime? CreationDate { get; set; }

    }
}
