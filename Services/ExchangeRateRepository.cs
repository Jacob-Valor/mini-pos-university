using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public ExchangeRateRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ExchangeRate?> GetLatestExchangeRateAsync()
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.LatestExchangeRate, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ExchangeRate
                {
                    Id = reader.GetInt32("id"),
                    UsdRate = reader.GetDecimal("dolar"),
                    ThbRate = reader.GetDecimal("bath"),
                    CreatedDate = reader.GetDateTime("ex_date")
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching latest exchange rate");
            return null;
        }
    }

    public async Task<List<ExchangeRate>> GetExchangeRateHistoryAsync()
    {
        var list = new List<ExchangeRate>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ExchangeRateHistory, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new ExchangeRate
                {
                    Id = reader.GetInt32("id"),
                    UsdRate = reader.GetDecimal("dolar"),
                    ThbRate = reader.GetDecimal("bath"),
                    CreatedDate = reader.GetDateTime("ex_date")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting exchange rate history");
        }
        return list;
    }

    public async Task<bool> AddExchangeRateAsync(ExchangeRate rate)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertExchangeRate, connection);
            command.Parameters.Add(new MySqlParameter("@usd", MySqlDbType.Decimal)
            {
                Precision = 7,
                Scale = 2,
                Value = rate.UsdRate
            });
            command.Parameters.Add(new MySqlParameter("@thb", MySqlDbType.Decimal)
            {
                Precision = 6,
                Scale = 2,
                Value = rate.ThbRate
            });
            command.Parameters.Add(new MySqlParameter("@date", MySqlDbType.DateTime)
            {
                Value = DateTime.Now
            });

            await command.ExecuteNonQueryAsync();
            Log.Information("Exchange rate added: USD={Usd}, THB={Thb}", rate.UsdRate, rate.ThbRate);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding exchange rate");
            return false;
        }
    }
}
