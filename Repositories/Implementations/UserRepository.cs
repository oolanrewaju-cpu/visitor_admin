using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly IDbConnection _db;
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM tblUsers WHERE Username = @Username",
                new { Username = username });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM tblUsers WHERE Email = @Email",
                new { Email = email });
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM tblUsers WHERE UserId = @UserId",
                new { UserId = id });
        }

        public async Task<User> CreateAsync(User user)
        {
            var sql = @"INSERT INTO tblUsers (Username, Email, PasswordHash, FirstName, LastName, Role, IsActive, IsEmailVerified, CreatedAt)
                        VALUES (@Username, @Email, @PasswordHash, @FirstName, @LastName, @Role, @IsActive, @IsEmailVerified, @CreatedAt)";
            
            return await _db.QuerySingleAsync<User>(sql, user);
        }

        public async Task SaveChangesAsync(User user)
        {
            var sql = @"UPDATE tblUsers SET
                            Username = @Username,
                            Email = @Email,
                            PasswordHash = @PasswordHash,
                            FirstName = @FirstName,
                            LastName = @LastName,
                            Role = @Role,
                            IsActive = @IsActive,
                            IsEmailVerified = @IsEmailVerified
                        WHERE UserId = @UserId";
            await _db.ExecuteAsync(sql, user);
        }
    }
}
