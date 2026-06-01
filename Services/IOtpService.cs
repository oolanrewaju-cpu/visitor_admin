namespace visitor_admin.Services
{
    public interface IOtpService
    {
        string GenerateOtp();
        Task SendOtpAsync(string email, string otp);
    }
}
