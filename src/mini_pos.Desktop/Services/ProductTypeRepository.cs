using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class ProductTypeRepository : IProductTypeRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public ProductTypeRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProductType>> GetProductTypesAsync()
    {
        var list = new List<ProductType>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ProductTypes, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductType
                {
                    Id = reader.GetString("category_id"),
                    Name = reader.GetString("category_name")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting product types");
        }
        return list;
    }

    public async Task<bool> AddProductTypeAsync(ProductType type)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertProductType, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);

            var rows = await command.ExecuteNonQueryAsync();
            if (rows != 1)
            {
                Log.Warning("Product type insert affected {Rows} rows for {Id}", rows, type.Id);
                return false;
            }

            Log.Information("Product type added: {Name}", type.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding product type: {Name}", type.Name);
            return false;
        }
    }

    public async Task<bool> UpdateProductTypeAsync(ProductType type)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateProductType, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);

            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0)
            {
                Log.Information("Product type updated: {Id}", type.Id);
                return true;
            }

            if (await ProductTypeExistsAsync(connection, type.Id))
            {
                Log.Information("Product type update skipped because data was unchanged: {Id}", type.Id);
                return true;
            }

            Log.Warning("Product type update failed because category was not found: {Id}", type.Id);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product type: {Id}", type.Id);
            return false;
        }
    }

    public async Task<bool> DeleteProductTypeAsync(string typeId)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteProductType, connection);
            command.Parameters.AddWithValue("@id", typeId);

            var rows = await command.ExecuteNonQueryAsync();
            if (rows != 1)
            {
                Log.Warning("Product type delete affected {Rows} rows for {Id}", rows, typeId);
                return false;
            }

            Log.Information("Product type deleted: {Id}", typeId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product type: {Id}", typeId);
            return false;
        }
    }

    private static async Task<bool> ProductTypeExistsAsync(MySqlConnection connection, string typeId)
    {
        await using var existsCommand = new MySqlCommand(SqlQueries.ProductTypeExists, connection);
        existsCommand.Parameters.AddWithValue("@id", typeId);
        var result = await existsCommand.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}
