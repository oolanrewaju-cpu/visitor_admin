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
        public void DeleteUser(User user)
        {
            _db.ExecuteAsync("DELETE FROM Users WHERE UserId = @UserId", new { UserId = user.UserId });
        }
        // This method retrieves all staff members from the database, allowing for filtering by name and a search query that can match the first name, surname, department, or email.
        public async Task<IEnumerable<User>> GetAllStaffAsync(string name, string searchQuery, int pageNumber, int pageSize)
        {
            var offset = (pageNumber - 1) * pageSize; // Calculate the offset for pagination
            return await _db.QueryAsync<User>("SELECT * FROM Users WHERE (Firstname LIKE @Name OR Surname LIKE @Name) AND (Firstname LIKE @SearchQuery OR Surname LIKE @SearchQuery or Department LIKE @SearchQuery or Email LIKE @SearchQuery) LIMIT @PageSize OFFSET @Offset",
                new
                {
                    Name = $"%{name}%",
                    SearchQuery = $"%{searchQuery}%",
                    PageSize = pageSize,
                    Offset = offset 
                });
        }
        // This method retrieves a staff member by their ID from the database.
        public async Task<User?> GetStaffByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE UserId = @UserId", new { UserId = id });
        }
        public async Task RegisterUser(User user)
        {
            await _db.ExecuteAsync("INSERT INTO Users (Username, Firstname, Surname, Email, DepartmentID, Password, RoleID, LastModifiedBy) VALUES (@Username, @Firstname, @Surname, @Email, @DepartmentID, @Password, @RoleID, @LastModifiedBy)",
                new
                {
                    user.Username,
                    user.Firstname,
                    user.Surname,
                    user.Email,
                    user.DepartmentID,
                    user.Password,
                    user.RoleID,
                    user.LastModifiedBy
                });
        }
        public async Task UpdateUser(User user)
        {
            await _db.ExecuteAsync("UPDATE Users SET Username = @Username, Firstname = @Firstname, Surname = @Surname, Email = @Email, DepartmentID = @DepartmentID, Password = @Password, RoleID = @RoleID, LastModifiedBy = @LastModifiedBy WHERE UserId = @UserId",
                new
                {
                    user.Username,
                    user.Firstname,
                    user.Surname,
                    user.Email,
                    user.DepartmentID,
                    user.Password,
                    user.RoleID,
                    user.LastModifiedBy,
                    user.UserId
                });
        }
    }
}
