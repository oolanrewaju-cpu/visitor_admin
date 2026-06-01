using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class EmailVerificationRepository : BaseRepository, IEmailVerificationRepository
    {
        private readonly IDbConnection _db;
        public EmailVerificationRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }

        public async Task<EmailVerification?> GetByTokenAsync(string token)
        {
            return await _db.QueryFirstOrDefaultAsync<EmailVerification>(
                "SELECT * FROM tblEmailVerifications WHERE Token = @Token AND IsUsed = 0",
                new { Token = token });
        }

        public async Task CreateAsync(EmailVerification verification)
        {
            var sql = @"INSERT INTO tblEmailVerifications (UserId, Token, ExpiresAt, IsUsed, CreatedAt)
                        VALUES (@UserId, @Token, @ExpiresAt, @IsUsed, @CreatedAt)";
            await _db.ExecuteAsync(sql, verification);
        }

        public async Task SaveChangesAsync(EmailVerification verification)
        {
            var sql = @"UPDATE tblEmailVerifications SET
                            IsUsed = @IsUsed
                        WHERE Id = @Id";
            await _db.ExecuteAsync(sql, new { verification.Id, verification.IsUsed });
        }
    }
}
