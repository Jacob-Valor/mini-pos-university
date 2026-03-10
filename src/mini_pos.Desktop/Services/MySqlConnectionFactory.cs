using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using mini_pos.Configuration;

using MySqlConnector;

namespace mini_pos.Services;

public sealed class MySqlConnectionFactory : IMySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _connectionString = DatabaseConnectionStringResolver.Resolve(options.Value.DefaultConnection);
    }

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = DatabaseConnectionStringResolver.Resolve(connectionString);
    }

    public async Task<MySqlConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
