using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using MySqlConnector;

using Dapper;

namespace mini_pos.Services;

public abstract class BaseRepository
{
    protected readonly IMySqlConnectionFactory _connectionFactory;

    protected BaseRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    protected async Task<List<T>> QueryAsync<T>(string sql, object? param = null)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        var result = await connection.QueryAsync<T>(sql, param);
        return result.AsList();
    }

    protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    protected async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        return await connection.ExecuteAsync(sql, param);
    }

    protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        await using var connection = await _connectionFactory.OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<T>(sql, param);
    }
}
