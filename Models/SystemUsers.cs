using System.ComponentModel.DataAnnotations;

namespace BIPL_RAASTP2M.Models
{
    public class SystemUsers
    {
        [Key]
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string UserID { get; set; } = null!;
        //public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;  // Admin, Staff, Manager
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
