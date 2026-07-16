namespace visitor_admin.Services
{
    public interface IEmailService
    {
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string token, string? username);
        Task<bool> SendEmailAsync(string toEmail, string subject, string body);
    }
}
