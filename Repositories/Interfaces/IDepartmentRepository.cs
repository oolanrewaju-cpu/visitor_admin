using visitor_admin.Entities;

namespace visitor_admin.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync(string? name, string? searchQuery);
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task CreateDepartment(Department department);
        Task UpdateDepartment(Department department);
        Task DeleteDepartmentAsync(int id);
        Task<Department?> GetByNameAsync(string name);
    }
}
