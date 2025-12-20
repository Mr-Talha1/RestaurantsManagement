namespace BIPL_RAASTP2M.DTO
{
    public class LoginResponseDto
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public LoginDataDto? Data { get; set; }
        public string? Token { get; set; }
    }
    public class LoginDataDto
    {
        public string UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Role { get; set; }
        public long? MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public string? LogoPath { get; set; }
    }
}
