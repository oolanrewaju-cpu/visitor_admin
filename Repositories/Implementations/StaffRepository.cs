using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class StaffRepository : BaseRepository, IStaffRepository
    {
        private readonly IDbConnection _db;
        public StaffRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }
        public async Task DeleteUserAsync(int id)
        {
            await _db.ExecuteAsync("DELETE FROM tblStaffList WHERE UserID = @UserID", new { UserID = id });
        }
        // This method retrieves all staff members from the database, allowing for filtering by name and a search query that can match the first name, surname, department, or email.
        public async Task<IEnumerable<Staff>> GetAllStaffAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT * FROM tblStaffList
                WHERE 
                    (
                        @Name IS NULL 
                        OR Firstname LIKE @Name 
                        OR Surname LIKE @Name
                    )
                    AND
                    (
                        @SearchQuery IS NULL
                        OR Firstname LIKE @SearchQuery 
                        OR Surname LIKE @SearchQuery 
                        OR Department LIKE @SearchQuery 
                        OR Email LIKE @SearchQuery
                    )
                ORDER BY Surname
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            return await _db.QueryAsync<Staff>(sql, new
            {
                Name = string.IsNullOrEmpty(name) ? null : $"%{name}%",
                SearchQuery = string.IsNullOrEmpty(searchQuery) ? null : $"%{searchQuery}%",
                PageSize = pageSize,
                Offset = offset
            });
        }
        // This method retrieves a staff member by their ID from the database.
        public async Task<Staff?> GetStaffByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<Staff>("SELECT * FROM tblStaffList WHERE UserID = @UserID", new { UserID = id });
        }
        public async Task RegisterUser(Staff user)
        {
            var sql = @"INSERT INTO tblStaffList (Username, Firstname, Surname, Email, Department, DepartmentID, Password, RoleID, RequestRoleID, StatusID)
                        OUTPUT INSERTED.UserID
                        VALUES (@Username, @Firstname, @Surname, @Email, @Department, @DepartmentID, @Password, @RoleID, @RequestRoleID, @StatusID)";
            user.UserID = await _db.ExecuteScalarAsync<int>(sql,
                new
                {
                    user.Username,
                    user.Firstname,
                    user.Surname,
                    user.Email,
                    user.Department,
                    user.DepartmentID,
                    user.Password,
                    user.RoleID,
                    user.RequestRoleID,
                    user.StatusID
                });
        }
        public async Task UpdateUser(Staff user)
        {
            var sql = @"
        UPDATE tblStaffList
        SET 
            Username = @Username,
            Firstname = @Firstname,
            Surname = @Surname,
            Email = @Email,
            Department = @Department,
            DepartmentID = @DepartmentID,
            Password = @Password,
            RoleID = @RoleID,
            RequestRoleID = @RequestRoleID,
            StatusID = @StatusID,
            LastModifiedBy = @LastModifiedBy
        WHERE UserID = @UserID"; // Do NOT update UserID

            await _db.ExecuteAsync(sql, new
            {
                user.Username,
                user.Firstname,
                user.Surname,
                user.Email,
                user.Department,
                user.DepartmentID,
                user.Password,
                user.RoleID,
                user.RequestRoleID,
                user.StatusID,
                user.LastModifiedBy,
                user.UserID // used only in WHERE clause
            });
        }
    }
}
