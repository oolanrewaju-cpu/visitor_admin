using Dapper;
using System.Data;
using visitor_admin.Entities;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Repositories.Implementations
{
    public class RequestRoleRepository : BaseRepository, IRequestRoleRepository
    {
        private readonly IDbConnection _db;
        public RequestRoleRepository(IConfiguration configuration) : base(configuration)
        {
            _db = CreateConnection();
        }

        public async Task<IEnumerable<RequestRole>> GetAllRequestRolesAsync()
        {
            var sql = "SELECT * FROM tblRequestRoles ORDER BY RequestRoleName";
            return await _db.QueryAsync<RequestRole>(sql);
        }

        public async Task<RequestRole?> GetRequestRoleByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<RequestRole>(
                "SELECT * FROM tblRequestRoles WHERE RequestRoleID = @RequestRoleID",
                new { RequestRoleID = id });
        }
    }
}
