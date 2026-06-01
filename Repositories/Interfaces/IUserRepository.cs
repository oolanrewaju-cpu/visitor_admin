using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task CreateAsync(User user);
        Task SaveChangesAsync(User user);
    }
}
