using MailKit.Net.Smtp;
using MimeKit;

namespace visitor_admin.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Get SMTP settings from configuration
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromName = _configuration["Email:FromName"] ?? "Nkay Fabs";
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;

                // Build the email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;

                // Set HTML body
                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using var client = new SmtpClient();

                _logger.LogInformation("Connecting to SMTP server {Host}:{Port}", smtpHost, smtpPort);
                await client.ConnectAsync(smtpHost, smtpPort, false);

                // Authenticate with SMTP credentials
                await client.AuthenticateAsync(smtpUser, smtpPassword);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string token, string? username)
        {
            var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7269";
            var resetUrl = $"{baseUrl}/api/auth/reset-password?token={token}";

            var subject = "LBS Visitors Admin - Reset your password";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request, {username}</h2>
                    <p>We received a request to reset your password. Click the link below to reset it:</p>
                    <p><a href=""{resetUrl}"" style=""background-color: #008CBA; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;"">Reset Password</a></p>
                    <p>Or copy and paste this link: {resetUrl}</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you did not request a password reset, please ignore this email or contact support.</p>
                </body>
                </html>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
