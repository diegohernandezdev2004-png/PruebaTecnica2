using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CursosAPI.Infrastructure.Data;

public class SqlDbContext
{
    private readonly string _connectionString;

    public SqlDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no configurada.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
