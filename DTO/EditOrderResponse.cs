namespace TBAppBackend.DTO
{
    public class EditOrderResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public OrderHistoryResponse? UpdatedOrder { get; set; }
    }
}
