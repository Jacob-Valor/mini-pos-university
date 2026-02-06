using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

namespace mini_pos.Services;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public CustomerRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        var customers = new List<Customer>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Customers, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new Customer
                {
                    Id = reader.GetString("cus_id"),
                    Name = reader.IsDBNull("cus_name") ? "" : reader.GetString("cus_name"),
                    Surname = reader.GetString("cus_lname"),
                    Gender = reader.GetString("gender"),
                    Address = reader.GetString("address"),
                    PhoneNumber = reader.IsDBNull("tel") ? "" : reader.GetString("tel")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching customers");
        }

        return customers;
    }

    public async Task<List<Customer>> SearchCustomersAsync(string keyword)
    {
        var list = new List<Customer>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.SearchCustomers, connection);
            command.Parameters.AddWithValue("@kw", $"%{keyword}%");

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Customer
                {
                    Id = reader.GetString("cus_id"),
                    Name = reader.IsDBNull("cus_name") ? "" : reader.GetString("cus_name"),
                    Surname = reader.GetString("cus_lname"),
                    Gender = reader.GetString("gender"),
                    Address = reader.GetString("address"),
                    PhoneNumber = reader.IsDBNull("tel") ? "" : reader.GetString("tel")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error searching customers with keyword: {Keyword}", keyword);
        }

        return list;
    }

    public async Task<bool> AddCustomerAsync(Customer customer)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertCustomer, connection);
            command.Parameters.AddWithValue("@id", customer.Id);
            command.Parameters.AddWithValue("@name", customer.Name);
            command.Parameters.AddWithValue("@surname", customer.Surname);
            command.Parameters.AddWithValue("@gender", customer.Gender);
            command.Parameters.AddWithValue("@addr", customer.Address);
            command.Parameters.AddWithValue("@tel", customer.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            Log.Information("Customer added: {Name} {Surname}", customer.Name, customer.Surname);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding customer");
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(Customer customer)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateCustomer, connection);
            command.Parameters.AddWithValue("@id", customer.Id);
            command.Parameters.AddWithValue("@name", customer.Name);
            command.Parameters.AddWithValue("@surname", customer.Surname);
            command.Parameters.AddWithValue("@gender", customer.Gender);
            command.Parameters.AddWithValue("@addr", customer.Address);
            command.Parameters.AddWithValue("@tel", customer.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            Log.Information("Customer updated: {Id}", customer.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating customer: {Id}", customer.Id);
            return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string customerId)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteCustomer, connection);
            command.Parameters.AddWithValue("@id", customerId);
            await command.ExecuteNonQueryAsync();
            Log.Information("Customer deleted: {Id}", customerId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting customer: {Id}", customerId);
            return false;
        }
    }
}
