namespace visitor_admin.Models.Dtos
{
    public class VerifyEmailRequestDto
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
