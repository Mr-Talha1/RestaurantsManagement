using System.ComponentModel.DataAnnotations;

namespace TBAppBackend.DTO
{
    public class AddBranchUserDto
    {
        public int BranchId { get; set; }
        [Required]
        public string UserID { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string MobileNumber { get; set; }      
        [Required]
        public string Password { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool Active { get; set; } = true;
    }
}
