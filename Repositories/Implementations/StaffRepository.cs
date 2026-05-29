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
        // This method deletes a user from the database based on their UserId.
        public void DeleteUser(Staff user)
        {
            _db.ExecuteAsync("DELETE FROM tblStaffList WHERE UserId = @UserId", new { UserId = user.UserID });
        }
        // This method retrieves all staff members from the database, allowing for filtering by name and a search query that can match the first name, surname, department, or email.
        public async Task<IEnumerable<Staff>> GetAllStaffAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize; // Calculate the offset for pagination
            return await _db.QueryAsync<Staff>("SELECT * FROM tblStaffList WHERE (Firstname LIKE @Name OR Surname LIKE @Name) AND (Firstname LIKE @SearchQuery OR Surname LIKE @SearchQuery or Department LIKE @SearchQuery or Email LIKE @SearchQuery) LIMIT @PageSize OFFSET @Offset",
                new
                {
                    Name = $"%{name}%",
                    SearchQuery = $"%{searchQuery}%",
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
            await _db.ExecuteAsync("INSERT INTO tblStaffList (Username, Firstname, Surname, Email, DepartmentID, Password, RoleID, RequestRoleID, StatusID) VALUES (@Username, @Firstname, @Surname, @Email, @DepartmentID, @Password, @RoleID, @RequestRoleID, @StatusID)",
                new
                {
                    user.Username,
                    user.Firstname,
                    user.Surname,
                    user.Email,
                    user.DepartmentID,
                    user.Password,
                    user.RoleID,
                    user.RequestRoleID,
                    user.StatusID
                });
        }
        public async Task UpdateUser(Staff user)
        {
            await _db.ExecuteAsync("UPDATE tblStaffList SET Username = @Username, Firstname = @Firstname, Surname = @Surname, Email = @Email, DepartmentID = @DepartmentID, Password = @Password, RoleID = @RoleID, RequestRoleID = @RequestRoleID, StatusID = @StatusID WHERE UserID = @UserID",
                new
                {
                    user.Username,
                    user.Firstname,
                    user.Surname,
                    user.Email,
                    user.DepartmentID,
                    user.Password,
                    user.RoleID,
                    user.RequestRoleID,
                    user.StatusID,
                });
        }
    }
}
