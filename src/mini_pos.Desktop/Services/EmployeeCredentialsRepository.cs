using System;
using System.Threading.Tasks;

using MySqlConnector;

using Serilog;

namespace mini_pos.Services;

public sealed class EmployeeCredentialsRepository : IEmployeeCredentialsRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public EmployeeCredentialsRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> UpdatePasswordAsync(string empId, string newPasswordHash)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateEmployeePassword, connection);
            command.Parameters.AddWithValue("@pwd", newPasswordHash);
            command.Parameters.AddWithValue("@id", empId);

            int rows = await command.ExecuteNonQueryAsync();
            Log.Information("Password updated for employee {Id}", empId);
            return rows > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating password for employee {Id}", empId);
            return false;
        }
    }

    public async Task<string?> GetStoredPasswordHashAsync(string username)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.StoredPasswordHash, connection);
            command.Parameters.AddWithValue("@username", username);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching password hash for user {Username}", username);
            return null;
        }
    }
}
