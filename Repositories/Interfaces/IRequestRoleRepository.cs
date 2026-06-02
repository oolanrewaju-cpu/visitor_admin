using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IRequestRoleRepository
    {
        Task<IEnumerable<RequestRole>> GetAllRequestRolesAsync();

        Task<RequestRole?> GetRequestRoleByIdAsync(int id);
        Task<RequestRole?> GetByNameAsync(string name);
    }
}
