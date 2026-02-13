using System;
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
        _connectionString = options.Value.DefaultConnection;
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not found. " +
                "Please check appsettings.json ConnectionStrings:DefaultConnection or set ConnectionStrings__DefaultConnection environment variable.");
        }
    }

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<MySqlConnection> OpenConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
