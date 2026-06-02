using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        private readonly IDbConnection _db;
        public DepartmentRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync(string? name, string? searchQuery)
        {
            var sql = @"
                SELECT * FROM tblDepartments
                WHERE 
                    (
                        @Name IS NULL 
                        OR DepartmentName LIKE @Name
                    )
                    AND
                    (
                        @SearchQuery IS NULL
                        OR DepartmentName LIKE @SearchQuery 
                        OR DepartmentCode LIKE @SearchQuery 
                        OR DepartmentDescription LIKE @SearchQuery
                    )
                ORDER BY DepartmentName";
            return await _db.QueryAsync<Department>(sql, new
            {
                Name = string.IsNullOrEmpty(name) ? null : $"%{name}%",
                SearchQuery = string.IsNullOrEmpty(searchQuery) ? null : $"%{searchQuery}%",
            });
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<Department>(
                "SELECT * FROM tblDepartments WHERE DepartmentID = @DepartmentID",
                new { DepartmentID = id });
        }

        public async Task CreateDepartment(Department department)
        {
            await _db.ExecuteAsync(
                "INSERT INTO tblDepartments (DepartmentCode, DepartmentName, DepartmentDescription, LastModifiedBy) VALUES (@DepartmentCode, @DepartmentName, @DepartmentDescription, @LastModifiedBy)",
                new { department.DepartmentCode, department.DepartmentName, department.DepartmentDescription, department.LastModifiedBy });
        }

        public async Task UpdateDepartment(Department department)
        {
            await _db.ExecuteAsync(
                "UPDATE tblDepartments SET DepartmentCode = @DepartmentCode, DepartmentName = @DepartmentName, DepartmentDescription = @DepartmentDescription, LastModifiedBy = @LastModifiedBy WHERE DepartmentID = @DepartmentID",
                new { department.DepartmentCode, department.DepartmentName, department.DepartmentDescription, department.LastModifiedBy, department.DepartmentID });
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            await _db.ExecuteAsync("DELETE FROM tblDepartments WHERE DepartmentID = @DepartmentID",
                new { DepartmentID = id });
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _db.QueryFirstOrDefaultAsync<Department>(
                "SELECT * FROM tblDepartments WHERE DepartmentName = @DepartmentName",
                new { DepartmentName = name });
        }
    }
}
