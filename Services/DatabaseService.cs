using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;

namespace mini_pos.Services;

/// <summary>
/// Provides database connectivity and operations for the Mini POS system.
/// Uses MySqlConnector for MariaDB/MySQL database access.
/// </summary>
public class DatabaseService
{
    private static DatabaseService? _instance;
    private static readonly object _lock = new();
    
    private readonly string _connectionString;

    /// <summary>
    /// Gets the singleton instance of DatabaseService.
    /// </summary>
    public static DatabaseService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DatabaseService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Private constructor - use Instance property to access.
    /// </summary>
    private DatabaseService()
    {
        _connectionString = ConfigurationService.Instance.GetConnectionString();
    }

    /// <summary>
    /// Reinitializes the singleton instance (useful for testing or config changes).
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }

    #region Connection Management

    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    /// <returns>An open MySqlConnection.</returns>
    public async Task<MySqlConnection> GetConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// Tests the database connection.
    /// </summary>
    /// <returns>True if connection successful, false otherwise.</returns>
    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            return (true, "ເຊື່ອມຕໍ່ຖານຂໍ້ມູນສຳເລັດ (Database connection successful)");
        }
        catch (MySqlException ex)
        {
            return (false, $"ເຊື່ອມຕໍ່ຖານຂໍ້ມູນລົ້ມເຫຼວ: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"ຂໍ້ຜິດພາດ: {ex.Message}");
        }
    }

    #endregion

    #region Employee Operations

    /// <summary>
    /// Validates employee login credentials.
    /// </summary>
    /// <param name="username">The username to validate.</param>
    /// <param name="passwordHash">The MD5 hashed password.</param>
    /// <returns>Employee object if valid, null otherwise.</returns>
    public async Task<Employee?> ValidateLoginAsync(string username, string passwordHash)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                SELECT emp_id, emp_name, emp_lname, gender, date_of_b, 
                       village_id, tel, start_date, username, status
                FROM employee 
                WHERE username = @username AND password = @password";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", passwordHash);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    Id = reader.GetString("emp_id"),
                    Name = reader.GetString("emp_name"),
                    Surname = reader.GetString("emp_lname"),
                    Gender = reader.GetString("gender"),
                    DateOfBirth = reader.GetDateTime("date_of_b"),
                    PhoneNumber = reader.GetString("tel"),
                    Position = reader.GetString("status")
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Login validation error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all employees from the database.
    /// </summary>
    /// <returns>List of Employee objects.</returns>
    public async Task<List<Employee>> GetEmployeesAsync()
    {
        var employees = new List<Employee>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                SELECT e.emp_id, e.emp_name, e.emp_lname, e.gender, e.date_of_b,
                       e.tel, e.start_date, e.username, e.status,
                       v.vname as village_name, d.distname as district_name, p.provname as province_name
                FROM employee e
                LEFT JOIN villages v ON e.village_id = v.vid
                LEFT JOIN districts d ON v.distid = d.distid
                LEFT JOIN provinces p ON d.provid = p.provid
                ORDER BY e.emp_id";

            await using var command = new MySqlCommand(query, connection);
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
                    Position = reader.GetString("status")
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching employees: {ex.Message}");
        }
        return employees;
    }

    #endregion

    #region Product Operations

    /// <summary>
    /// Gets all products from the database.
    /// </summary>
    /// <returns>List of Product objects.</returns>
    public async Task<List<Product>> GetProductsAsync()
    {
        var products = new List<Product>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                ORDER BY p.barcode";

            await using var command = new MySqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Barcode = reader.GetString("barcode"),
                    ProductName = reader.GetString("product_name"),
                    Unit = reader.GetString("unit"),
                    Quantity = reader.GetInt32("quantity"),
                    QuantityMin = reader.GetInt32("quantity_min"),
                    CostPrice = reader.GetDecimal("cost_price"),
                    RetailPrice = reader.GetDecimal("retail_price"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching products: {ex.Message}");
        }
        return products;
    }

    /// <summary>
    /// Gets a product by barcode.
    /// </summary>
    /// <param name="barcode">The product barcode.</param>
    /// <returns>Product object if found, null otherwise.</returns>
    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                WHERE p.barcode = @barcode";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@barcode", barcode);
            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Barcode = reader.GetString("barcode"),
                    ProductName = reader.GetString("product_name"),
                    Unit = reader.GetString("unit"),
                    Quantity = reader.GetInt32("quantity"),
                    QuantityMin = reader.GetInt32("quantity_min"),
                    CostPrice = reader.GetDecimal("cost_price"),
                    RetailPrice = reader.GetDecimal("retail_price"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Customer Operations

    /// <summary>
    /// Gets all customers from the database.
    /// </summary>
    /// <returns>List of Customer objects.</returns>
    public async Task<List<Customer>> GetCustomersAsync()
    {
        var customers = new List<Customer>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                SELECT cus_id, cus_name, cus_lname, gender, address, tel
                FROM customer
                ORDER BY cus_id";

            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error fetching customers: {ex.Message}");
        }
        return customers;
    }

    #endregion

    #region Brand Operations

    /// <summary>
    /// Gets all brands from the database.
    /// </summary>
    /// <returns>List of Brand objects.</returns>
    public async Task<List<Brand>> GetBrandsAsync()
    {
        var brands = new List<Brand>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT brand_id, brand_name FROM brand ORDER BY brand_id";

            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error fetching brands: {ex.Message}");
        }
        return brands;
    }

    #endregion

    #region Generic Query Methods

    /// <summary>
    /// Executes a non-query SQL command (INSERT, UPDATE, DELETE).
    /// </summary>
    /// <param name="query">The SQL query.</param>
    /// <param name="parameters">Query parameters.</param>
    /// <returns>Number of rows affected.</returns>
    public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing query: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// Executes a scalar query and returns a single value.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="query">The SQL query.</param>
    /// <param name="parameters">Query parameters.</param>
    /// <returns>The scalar value or default.</returns>
    public async Task<T?> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return default;

            return (T)Convert.ChangeType(result, typeof(T));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing scalar query: {ex.Message}");
            return default;
        }
    }

    #endregion
}
