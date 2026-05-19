using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.Models
{
    public class UserRoles
    {
        [Key]
        public int Id { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
    }
}
