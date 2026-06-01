using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IEmailVerificationRepository
    {
        Task<EmailVerification?> GetByTokenAsync(string token);
        Task CreateAsync(EmailVerification verification);
        Task SaveChangesAsync(EmailVerification verification);
    }
}
