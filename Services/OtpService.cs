namespace visitor_admin.Services
{
    public class OtpService : IOtpService
    {
        private readonly ILogger<OtpService> _logger;

        public OtpService(ILogger<OtpService> logger)
        {
            _logger = logger;
        }

        public string GenerateOtp()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        public Task SendOtpAsync(string email, string otp)
        {
            _logger.LogInformation("OTP sent to {Email}: {Otp}", email, otp);
            return Task.CompletedTask;
        }
    }
}
