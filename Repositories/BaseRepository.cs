using Microsoft.Data.SqlClient;
using System.Data;

namespace visitor_admin.Repositories
{
    public abstract class BaseRepository : IDisposable
    {
        private readonly string _connectionString;
        private IDbConnection? _connection;

        public BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        protected IDbConnection CreateConnection()
        {
            _connection = new SqlConnection(_connectionString);
            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
