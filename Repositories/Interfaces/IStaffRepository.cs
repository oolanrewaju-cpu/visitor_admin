using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Staff>> GetAllStaffAsync(string? name, string? searchQuery, int pageNumber, int pageSize);

        Task<Staff?> GetStaffByIdAsync(int id);

        Task RegisterUser(Staff user);

        Task UpdateUser(Staff user);

        Task DeleteUserAsync(int id);

        Task<IEnumerable<string>> GetAllUsernamesAsync();

        Task<IEnumerable<string>> GetAllEmailsAsync();

        Task BulkRegisterUsersAsync(IEnumerable<Staff> users);
    }
}
