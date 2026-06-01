namespace visitor_admin.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string email, string token)
        {
            _logger.LogInformation("Password reset email sent to {Email} with token: {Token}", email, token);
            return Task.CompletedTask;
        }
    }
}
