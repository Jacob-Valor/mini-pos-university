using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using mini_pos.Models;

using MySqlConnector;

using Serilog;

namespace mini_pos.Services;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly IMySqlConnectionFactory _connectionFactory;

    public EmployeeRepository(IMySqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string MaskUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return "[empty]";
        }

        if (username.Length <= 2)
        {
            return "***";
        }

        return username.Substring(0, 2) + new string('*', username.Length - 2);
    }

    public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.EmployeeByUsername, connection);
            command.Parameters.AddWithValue("@username", username);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Log.Information("User lookup succeeded for {Username}", MaskUsername(username));
                return new Employee
                {
                    Id = reader.GetString("emp_id"),
                    Name = reader.GetString("emp_name"),
                    Surname = reader.GetString("emp_lname"),
                    Gender = reader.GetString("gender"),
                    DateOfBirth = reader.GetDateTime("date_of_b"),
                    PhoneNumber = reader.GetString("tel"),
                    Province = reader.IsDBNull("province_name") ? string.Empty : reader.GetString("province_name"),
                    District = reader.IsDBNull("district_name") ? string.Empty : reader.GetString("district_name"),
                    Village = reader.IsDBNull("village_name") ? string.Empty : reader.GetString("village_name"),
                    ProvinceId = reader.IsDBNull("province_id") ? string.Empty : reader.GetString("province_id"),
                    DistrictId = reader.IsDBNull("district_id") ? string.Empty : reader.GetString("district_id"),
                    VillageId = reader.IsDBNull("village_id") ? string.Empty : reader.GetString("village_id"),
                    Position = reader.GetString("status"),
                    Username = reader.GetString("username")
                };
            }

            Log.Warning("User lookup failed for {Username}", MaskUsername(username));
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching user by username: {Username}", MaskUsername(username));
            return null;
        }
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        var employees = new List<Employee>();
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.Employees, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetString("emp_id"),
                    Name = reader.GetString("emp_name"),
                    Surname = reader.GetString("emp_lname"),
                    Gender = reader.GetString("gender"),
                    DateOfBirth = reader.GetDateTime("date_of_b"),
                    PhoneNumber = reader.GetString("tel"),
                    Village = reader.IsDBNull("village_name") ? "" : reader.GetString("village_name"),
                    District = reader.IsDBNull("district_name") ? "" : reader.GetString("district_name"),
                    Province = reader.IsDBNull("province_name") ? "" : reader.GetString("province_name"),
                    VillageId = reader.IsDBNull("village_id") ? string.Empty : reader.GetString("village_id"),
                    DistrictId = reader.IsDBNull("district_id") ? string.Empty : reader.GetString("district_id"),
                    ProvinceId = reader.IsDBNull("province_id") ? string.Empty : reader.GetString("province_id"),
                    Position = reader.GetString("status"),
                    Username = reader.GetString("username")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching employees");
        }

        return employees;
    }

    public async Task<bool> AddEmployeeAsync(Employee emp)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();

            var villageId = !string.IsNullOrWhiteSpace(emp.VillageId) ? emp.VillageId : emp.Village;
            if (string.IsNullOrWhiteSpace(villageId))
            {
                Log.Warning("Employee missing village id for {Id}", emp.Id);
                return false;
            }

            await using var command = new MySqlCommand(SqlQueries.InsertEmployee, connection);
            command.Parameters.AddWithValue("@id", emp.Id);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.Add(new MySqlParameter("@dob", MySqlDbType.Date)
            {
                Value = emp.DateOfBirth.Date
            });
            command.Parameters.AddWithValue("@vid", villageId);
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.Add(new MySqlParameter("@start", MySqlDbType.Date)
            {
                Value = DateTime.UtcNow.Date
            });
            command.Parameters.AddWithValue("@user", emp.Username);
            command.Parameters.AddWithValue("@pass", emp.Password);
            command.Parameters.AddWithValue("@status", emp.Position);

            await command.ExecuteNonQueryAsync();
            Log.Information("Employee added: {Name} {Surname}", emp.Name, emp.Surname);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding employee: {Name} {Surname}", emp.Name, emp.Surname);
            return false;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(Employee emp)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();

            var villageId = !string.IsNullOrWhiteSpace(emp.VillageId) ? emp.VillageId : emp.Village;
            if (string.IsNullOrWhiteSpace(villageId))
            {
                Log.Warning("Employee missing village id for {Id}", emp.Id);
                return false;
            }

            await using var command = new MySqlCommand(SqlQueries.UpdateEmployee, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.Add(new MySqlParameter("@dob", MySqlDbType.Date)
            {
                Value = emp.DateOfBirth.Date
            });
            command.Parameters.AddWithValue("@vid", villageId);
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@status", emp.Position);
            command.Parameters.AddWithValue("@id", emp.Id);

            await command.ExecuteNonQueryAsync();
            Log.Information("Employee updated: {Id}", emp.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating employee: {Id}", emp.Id);
            return false;
        }
    }

    public async Task<bool> DeleteEmployeeAsync(string empId)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteEmployee, connection);
            command.Parameters.AddWithValue("@id", empId);
            await command.ExecuteNonQueryAsync();
            Log.Information("Employee deleted: {Id}", empId);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting employee: {Id}", empId);
            return false;
        }
    }

    public async Task<bool> UpdateEmployeeProfileAsync(Employee emp)
    {
        try
        {
            await using var connection = await _connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateEmployeeProfile, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.Add(new MySqlParameter("@dob", MySqlDbType.Date)
            {
                Value = emp.DateOfBirth.Date
            });
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@username", emp.Username);
            command.Parameters.AddWithValue("@id", emp.Id);

            int rows = await command.ExecuteNonQueryAsync();
            Log.Information("Profile updated for employee {Id}", emp.Id);
            return rows > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating profile for employee {Id}", emp.Id);
            return false;
        }
    }
}
