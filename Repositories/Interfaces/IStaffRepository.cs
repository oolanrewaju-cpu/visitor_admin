using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<User>> GetAllStaffAsync(string name, string searchQuery, int pageNumber, int pageSize);

        Task<User?> GetStaffByIdAsync(int id);

        Task RegisterUser(User user);

        Task UpdateUser(User user);

        void DeleteUser(User user);

        // Dapper has no need for saveChangesAsync
    }
}
