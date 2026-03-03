namespace BIPL_RAASTP2M.DTO
{
    public class EditOrderResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public OrderHistoryResponse? UpdatedOrder { get; set; }
    }
}
