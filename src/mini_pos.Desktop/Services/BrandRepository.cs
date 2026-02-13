using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class BrandRepository : IBrandRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public BrandRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        var brands = new List<Brand>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Brands, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                brands.Add(new Brand
                {
                    Id = reader.GetString("brand_id"),
                    Name = reader.GetString("brand_name")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching brands");
        }

        return brands;
    }

    public async Task<bool> AddBrandAsync(Brand brand)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertBrand, connection);
            command.Parameters.AddWithValue("@id", brand.Id);
            command.Parameters.AddWithValue("@name", brand.Name);
            await command.ExecuteNonQueryAsync();
            Log.Information("Brand added: {Name}", brand.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding brand: {Name}", brand.Name);
            return false;
        }
    }

    public async Task<bool> UpdateBrandAsync(Brand brand)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateBrand, connection);
            command.Parameters.AddWithValue("@id", brand.Id);
            command.Parameters.AddWithValue("@name", brand.Name);
            await command.ExecuteNonQueryAsync();
            Log.Information("Brand updated: {Id}", brand.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating brand: {Id}", brand.Id);
            return false;
        }
    }

    public async Task<bool> DeleteBrandAsync(string brandId)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteBrand, connection);
            command.Parameters.AddWithValue("@id", brandId);
            await command.ExecuteNonQueryAsync();
            Log.Information("Brand deleted: {Id}", brandId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting brand: {Id}", brandId);
            return false;
        }
    }
}
