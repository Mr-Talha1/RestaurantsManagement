namespace TBAppBackend.DTO
{
    public class LoginRequestDto
    {
        public string UserId { get; set; } = null!;
        public string? Password { get; set; }
    }
}
