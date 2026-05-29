using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Staff>> GetAllStaffAsync(string? name, string? searchQuery, int pageNumber, int pageSize);

        Task<Staff?> GetStaffByIdAsync(int id);

        Task RegisterUser(Staff user);

        Task UpdateUser(Staff user);

        void DeleteUser(Staff user);

        // Dapper has no need for saveChangesAsync
    }
}
