namespace visitor_admin.Models.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public AdminUserDto AdminUser { get; set; } = null!;
    }
}
