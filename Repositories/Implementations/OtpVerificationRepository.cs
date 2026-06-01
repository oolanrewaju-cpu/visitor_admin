using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class OtpVerificationRepository : BaseRepository, IOtpVerificationRepository
    {
        private readonly IDbConnection _db;
        public OtpVerificationRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }

        public async Task<OtpVerification?> GetValidOtpAsync(int userId, string otpCode)
        {
            return await _db.QueryFirstOrDefaultAsync<OtpVerification>(
                "SELECT * FROM tblOtpVerifications WHERE UserId = @UserId AND OtpCode = @OtpCode AND IsUsed = 0 AND ExpiresAt > @Now",
                new { UserId = userId, OtpCode = otpCode, Now = DateTime.UtcNow });
        }

        public async Task CreateAsync(OtpVerification otp)
        {
            var sql = @"INSERT INTO tblOtpVerifications (UserId, OtpCode, ExpiresAt, IsUsed, CreatedAt)
                        VALUES (@UserId, @OtpCode, @ExpiresAt, @IsUsed, @CreatedAt)";
            await _db.ExecuteAsync(sql, otp);
        }

        public async Task SaveChangesAsync(OtpVerification otp)
        {
            var sql = @"UPDATE tblOtpVerifications SET
                            IsUsed = @IsUsed
                        WHERE Id = @Id";
            await _db.ExecuteAsync(sql, new { otp.Id, otp.IsUsed });
        }
    }
}
