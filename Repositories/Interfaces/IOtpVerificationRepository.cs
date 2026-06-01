using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IOtpVerificationRepository
    {
        Task<OtpVerification?> GetValidOtpAsync(int userId, string otpCode);
        Task CreateAsync(OtpVerification otp);
        Task SaveChangesAsync(OtpVerification otp);
    }
}
