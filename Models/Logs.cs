using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class Logs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 ID { get; set; }
        public string UserID { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string Application { get; set; }
        public string Interface { get; set; }
        public DateTime eDate { get; set; }
        public string IPAddress { get; set; }
    }
}
