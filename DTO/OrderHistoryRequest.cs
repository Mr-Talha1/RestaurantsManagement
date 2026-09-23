namespace TBAppBackend.DTO
{
    public class OrderHistoryRequest
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int? BranchId { get; set; }
        //public string OrderType { get; set; } // All, Dining, Takea
    }
}
