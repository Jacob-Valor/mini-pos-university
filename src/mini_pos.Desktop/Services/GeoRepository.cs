using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class GeoRepository : IGeoRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public GeoRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Province>> GetProvincesAsync()
    {
        var list = new List<Province>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Provinces, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Province
                {
                    Id = reader.GetString("provid"),
                    Name = reader.GetString("provname")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting provinces");
        }

        return list;
    }

    public async Task<List<District>> GetDistrictsByProvinceAsync(string provinceId)
    {
        var list = new List<District>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DistrictsByProvince, connection);
            command.Parameters.AddWithValue("@pid", provinceId);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new District
                {
                    Id = reader.GetString("distid"),
                    Name = reader.GetString("distname"),
                    ProvinceId = reader.GetString("provid")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting districts for province: {ProvinceId}", provinceId);
        }

        return list;
    }

    public async Task<List<Village>> GetVillagesByDistrictAsync(string districtId)
    {
        var list = new List<Village>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.VillagesByDistrict, connection);
            command.Parameters.AddWithValue("@did", districtId);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Village
                {
                    Id = reader.GetString("vid"),
                    Name = reader.GetString("vname"),
                    DistrictId = reader.GetString("distid")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting villages for district: {DistrictId}", districtId);
        }

        return list;
    }
}
