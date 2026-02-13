using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public SupplierRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        var list = new List<Supplier>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Suppliers, connection);
            await using var reader = await command.ExecuteReaderAsync();

            int seq = 1;
            while (await reader.ReadAsync())
            {
                list.Add(new Supplier
                {
                    Sequence = seq++,
                    Id = reader.GetString("sup_id"),
                    Name = reader.GetString("sup_name"),
                    ContactName = reader.IsDBNull("contract_name") ? "" : reader.GetString("contract_name"),
                    Email = reader.IsDBNull("email") ? "" : reader.GetString("email"),
                    Phone = reader.IsDBNull("telephone") ? "" : reader.GetString("telephone"),
                    Address = reader.IsDBNull("address") ? "" : reader.GetString("address")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting suppliers");
        }

        return list;
    }

    public async Task<bool> AddSupplierAsync(Supplier supplier)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertSupplier, connection);
            command.Parameters.AddWithValue("@id", supplier.Id);
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@contact", supplier.ContactName);
            command.Parameters.AddWithValue("@email", supplier.Email);
            command.Parameters.AddWithValue("@tel", supplier.Phone);
            command.Parameters.AddWithValue("@addr", supplier.Address);

            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier added: {Name}", supplier.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding supplier: {Name}", supplier.Name);
            return false;
        }
    }

    public async Task<bool> UpdateSupplierAsync(Supplier supplier)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateSupplier, connection);
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@contact", supplier.ContactName);
            command.Parameters.AddWithValue("@email", supplier.Email);
            command.Parameters.AddWithValue("@tel", supplier.Phone);
            command.Parameters.AddWithValue("@addr", supplier.Address);
            command.Parameters.AddWithValue("@id", supplier.Id);

            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier updated: {Id}", supplier.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating supplier: {Id}", supplier.Id);
            return false;
        }
    }

    public async Task<bool> DeleteSupplierAsync(string supplierId)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteSupplier, connection);
            command.Parameters.AddWithValue("@id", supplierId);
            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier deleted: {Id}", supplierId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting supplier: {Id}", supplierId);
            return false;
        }
    }
}
