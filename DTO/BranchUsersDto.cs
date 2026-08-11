namespace TBAppBackend.DTO
{
    public class BranchUsersDto
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string Address { get; set; }
        public bool? Active { get; set; }
        public int? CityID { get; set; }
        public string? CreationDate { get; set; }
        public List<UserDto> Users { get; set; }
    }
    public class UserDto
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }
        public string FullName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string? CreationDate { get; set; }

    }
}
