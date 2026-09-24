namespace TBAppBackend.DTO
{
    public class DashboardRequestDto
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int? BranchId { get; set; }
    }
}
