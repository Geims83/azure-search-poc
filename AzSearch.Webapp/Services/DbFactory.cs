using Microsoft.Data.SqlClient;

namespace AzSearch.App.Services
{
    public interface IDbFactory
    {
        SqlConnection GetConnection();
    }

    public class DbFactory : IDbFactory
    {
        private readonly string _connectionString;
        public DbFactory() { }

        public DbFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
