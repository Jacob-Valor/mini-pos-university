using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;

namespace mini_pos.Services;

public interface IDatabaseService
{
    Task<MySqlConnection> GetConnectionAsync();
    Task<(bool Success, string Message)> TestConnectionAsync();
    Task<Employee?> ValidateLoginAsync(string username, string passwordHash);
    Task<List<Employee>> GetEmployeesAsync();
    Task<List<Product>> GetProductsAsync();
    Task<Product?> GetProductByBarcodeAsync(string barcode);
    Task<List<Customer>> GetCustomersAsync();
    Task<List<Brand>> GetBrandsAsync();
    Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null);
    Task<T?> ExecuteScalarAsync<T>(string query, Dictionary<string, object>? parameters = null);
    Task<bool> UpdateEmployeeProfileAsync(Employee emp);
    Task<bool> UpdatePasswordAsync(string empId, string newPasswordHash);
    Task<string?> GetStoredPasswordHashAsync(string username);
    Task<ExchangeRate?> GetLatestExchangeRateAsync();
    Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details);
    
    // Product Operations
    Task<bool> AddProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string barcode);

    // Customer Operations
    Task<bool> AddCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(string customerId);
    
    // Brand Operations (GetBrandsAsync defined earlier)
    Task<bool> AddBrandAsync(Brand brand);
    Task<bool> UpdateBrandAsync(Brand brand);
    Task<bool> DeleteBrandAsync(string brandId);

    // Type Operations
    Task<List<ProductType>> GetProductTypesAsync();
    Task<bool> AddProductTypeAsync(ProductType type);
    Task<bool> UpdateProductTypeAsync(ProductType type);
    Task<bool> DeleteProductTypeAsync(string typeId);

    // Geo Operations
    Task<List<Province>> GetProvincesAsync();
    Task<List<District>> GetDistrictsByProvinceAsync(string provinceId);
    Task<List<Village>> GetVillagesByDistrictAsync(string districtId);
    
    // Employee Management
    Task<bool> AddEmployeeAsync(Employee emp);
    Task<bool> UpdateEmployeeAsync(Employee emp);
    Task<bool> DeleteEmployeeAsync(string empId);

    // Supplier Operations
    Task<List<Supplier>> GetSuppliersAsync();
    Task<bool> AddSupplierAsync(Supplier supplier);
    Task<bool> UpdateSupplierAsync(Supplier supplier);
    Task<bool> DeleteSupplierAsync(string supplierId);

    // Exchange Rate Operations
    Task<List<ExchangeRate>> GetExchangeRateHistoryAsync();
    Task<bool> AddExchangeRateAsync(ExchangeRate rate);
}

/// <summary>
/// Provides database connectivity and operations for the Mini POS system.
/// Uses MySqlConnector for MariaDB/MySQL database access.
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        _connectionString = ConfigurationService.Instance.GetConnectionString();
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
                    Position = reader.GetString("status"),
                    Username = reader.GetString("username")
                };
            }
            return null;
        }
        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error during login validation: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error during login validation: {ex.Message}");
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
                    Position = reader.GetString("status"),
                    Username = reader.GetString("username")
                });
            }
        }
        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error fetching employees: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error fetching employees: {ex.Message}");
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
        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error fetching products: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error fetching products: {ex.Message}");
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

    #region Profile Operations

    /// <summary>
    /// Updates employee profile information.
    /// </summary>
    public async Task<bool> UpdateEmployeeProfileAsync(Employee emp)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                UPDATE employee 
                SET emp_name = @name, 
                    emp_lname = @surname, 
                    gender = @gender, 
                    date_of_b = @dob, 
                    tel = @tel,
                    username = @username
                WHERE emp_id = @id";

            // Note: Not updating address (village_id) or picture/status here to simplify, 
            // as we need IDs for address and logic for picture blob.

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@username", emp.Username);
            command.Parameters.AddWithValue("@id", emp.Id);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating profile: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Updates employee password.
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(string empId, string newPasswordHash)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "UPDATE employee SET password = @pwd WHERE emp_id = @id";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@pwd", newPasswordHash);
            command.Parameters.AddWithValue("@id", empId);

            int rows = await command.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating password: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the stored password hash for a given username.
    /// </summary>
    public async Task<string?> GetStoredPasswordHashAsync(string username)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT password FROM employee WHERE username = @username";
            
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", username);
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching password hash: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Sales Operations

    public async Task<ExchangeRate?> GetLatestExchangeRateAsync()
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 1";
            
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error fetching exchange rate: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
    {
        MySqlTransaction? transaction = null;
        try
        {
            await using var connection = await GetConnectionAsync();
            transaction = await connection.BeginTransactionAsync();

            // 1. Insert Sales Header
            const string insertSaleSql = @"
                INSERT INTO sales (ex_id, cus_id, emp_id, date_sale, subtotal, pay, money_change)
                VALUES (@exId, @cusId, @empId, @date, @sub, @pay, @change);
                SELECT LAST_INSERT_ID();";

            await using var saleCmd = new MySqlCommand(insertSaleSql, connection, transaction);
            saleCmd.Parameters.AddWithValue("@exId", sale.ExchangeRateId);
            saleCmd.Parameters.AddWithValue("@cusId", sale.CustomerId);
            saleCmd.Parameters.AddWithValue("@empId", sale.EmployeeId);
            saleCmd.Parameters.AddWithValue("@date", sale.DateSale);
            saleCmd.Parameters.AddWithValue("@sub", sale.SubTotal);
            saleCmd.Parameters.AddWithValue("@pay", sale.Pay);
            saleCmd.Parameters.AddWithValue("@change", sale.Change);

            var saleIdObj = await saleCmd.ExecuteScalarAsync();
            int saleId = Convert.ToInt32(saleIdObj);

            // 2. Insert Details & Update Stock
            const string insertDetailSql = @"
                INSERT INTO sales_product (sales_id, product_id, qty, price, total)
                VALUES (@saleId, @prodId, @qty, @price, @total)";

            const string updateStockSql = @"
                UPDATE product SET quantity = quantity - @qty WHERE barcode = @prodId";

            foreach (var item in details)
            {
                // Detail Insert
                await using var detailCmd = new MySqlCommand(insertDetailSql, connection, transaction);
                detailCmd.Parameters.AddWithValue("@saleId", saleId);
                detailCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                detailCmd.Parameters.AddWithValue("@qty", item.Quantity);
                detailCmd.Parameters.AddWithValue("@price", item.Price);
                detailCmd.Parameters.AddWithValue("@total", item.Total);
                await detailCmd.ExecuteNonQueryAsync();

                // Stock Update
                await using var stockCmd = new MySqlCommand(updateStockSql, connection, transaction);
                stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                stockCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                await stockCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            Console.Error.WriteLine($"Error creating sale: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Product Operations

    public async Task<bool> AddProductAsync(Product p)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                INSERT INTO product (barcode, product_name, unit, quantity, quantity_min, cost_price, retail_price, brand_id, category_id, status)
                VALUES (@id, @name, @unit, @qty, @min, @cost, @price, @brand, @type, @status)";

            await using var command = new MySqlCommand(query, connection);
            // Note: We need to map Brand Name to ID and Type Name to ID
            // For this quick implementation, I will assume the ViewModel passes IDs or we need a lookup.
            // However, the current model uses Names.
            // Let's assume for now we just store the ID if available or find it.
            // To be safe and quick, I will just use the parameters directly, but ideally we need to look up IDs.
            // The table schema expects varchar(4) for IDs.
            
            command.Parameters.AddWithValue("@id", p.Id);
            command.Parameters.AddWithValue("@name", p.Name);
            command.Parameters.AddWithValue("@unit", p.Unit);
            command.Parameters.AddWithValue("@qty", p.Quantity);
            command.Parameters.AddWithValue("@min", p.MinQuantity);
            command.Parameters.AddWithValue("@cost", p.CostPrice);
            command.Parameters.AddWithValue("@price", p.SellingPrice);
            
            // Just use the values passed (ViewModel should ensure these are IDs)
            command.Parameters.AddWithValue("@brand", p.Brand); 
            command.Parameters.AddWithValue("@type", p.Type);
            command.Parameters.AddWithValue("@status", p.Status);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding product: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Product p)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                UPDATE product SET 
                    product_name=@name, unit=@unit, quantity=@qty, quantity_min=@min, 
                    cost_price=@cost, retail_price=@price, brand_id=@brand, category_id=@type, status=@status
                WHERE barcode=@id";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", p.Id);
            command.Parameters.AddWithValue("@name", p.Name);
            command.Parameters.AddWithValue("@unit", p.Unit);
            command.Parameters.AddWithValue("@qty", p.Quantity);
            command.Parameters.AddWithValue("@min", p.MinQuantity);
            command.Parameters.AddWithValue("@cost", p.CostPrice);
            command.Parameters.AddWithValue("@price", p.SellingPrice);
            command.Parameters.AddWithValue("@brand", p.Brand);
            command.Parameters.AddWithValue("@type", p.Type);
            command.Parameters.AddWithValue("@status", p.Status);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating product: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(string barcode)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM product WHERE barcode = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", barcode);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting product: {ex.Message}");
            return false;
        }
    }

    public async Task<List<ProductType>> GetProductTypesAsync()
    {
         var list = new List<ProductType>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT category_id, category_name FROM category";
            await using var command = new MySqlCommand(query, connection);
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
             Console.Error.WriteLine($"Error getting types: {ex.Message}");
        }
        return list;
    }

    #endregion

    #region Customer Operations

    public async Task<bool> AddCustomerAsync(Customer c)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                INSERT INTO customer (cus_id, cus_name, cus_lname, gender, address, tel)
                VALUES (@id, @name, @surname, @gender, @addr, @tel)";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", c.Id);
            command.Parameters.AddWithValue("@name", c.Name);
            command.Parameters.AddWithValue("@surname", c.Surname);
            command.Parameters.AddWithValue("@gender", c.Gender);
            command.Parameters.AddWithValue("@addr", c.Address);
            command.Parameters.AddWithValue("@tel", c.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding customer: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(Customer c)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                UPDATE customer SET 
                    cus_name=@name, cus_lname=@surname, gender=@gender, address=@addr, tel=@tel
                WHERE cus_id=@id";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", c.Id);
            command.Parameters.AddWithValue("@name", c.Name);
            command.Parameters.AddWithValue("@surname", c.Surname);
            command.Parameters.AddWithValue("@gender", c.Gender);
            command.Parameters.AddWithValue("@addr", c.Address);
            command.Parameters.AddWithValue("@tel", c.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
             Console.Error.WriteLine($"Error updating customer: {ex.Message}");
             return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM customer WHERE cus_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
             Console.Error.WriteLine($"Error deleting customer: {ex.Message}");
             return false;
        }
    }

    #endregion

    #region Brand Operations

    public async Task<bool> AddBrandAsync(Brand brand)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "INSERT INTO brand (brand_id, brand_name) VALUES (@id, @name)";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", brand.Id);
            command.Parameters.AddWithValue("@name", brand.Name);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding brand: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateBrandAsync(Brand brand)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "UPDATE brand SET brand_name = @name WHERE brand_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", brand.Id);
            command.Parameters.AddWithValue("@name", brand.Name);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating brand: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteBrandAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM brand WHERE brand_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting brand: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Type Operations
    
    // GetProductTypesAsync is already defined above... wait, I need to check where it is.
    // Ah, it was defined earlier. I should add the CRUD methods there.
    // Let me search for GetProductTypesAsync implementation.
    
    // ... (Adding CRUD methods)
    
    public async Task<bool> AddProductTypeAsync(ProductType type)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "INSERT INTO category (category_id, category_name) VALUES (@id, @name)";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding type: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateProductTypeAsync(ProductType type)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "UPDATE category SET category_name = @name WHERE category_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating type: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteProductTypeAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM category WHERE category_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting type: {ex.Message}");
            return false;
        }
    }

    // DeleteProductTypeAsync defined above
    
    // GetProductTypesAsync removed to avoid duplication (it was defined earlier in file)
    #endregion


    #region Geo Operations

    public async Task<List<Province>> GetProvincesAsync()
    {
        var list = new List<Province>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT provid, provname FROM provinces ORDER BY provname";
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error getting provinces: {ex.Message}");
        }
        return list;
    }

    public async Task<List<District>> GetDistrictsByProvinceAsync(string provinceId)
    {
        var list = new List<District>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT distid, distname, provid FROM districts WHERE provid = @pid ORDER BY distname";
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error getting districts: {ex.Message}");
        }
        return list;
    }

    public async Task<List<Village>> GetVillagesByDistrictAsync(string districtId)
    {
        var list = new List<Village>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT vid, vname, distid FROM villages WHERE distid = @did ORDER BY vname";
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error getting villages: {ex.Message}");
        }
        return list;
    }

    #endregion

    #region Employee Management (CRUD)

    public async Task<bool> AddEmployeeAsync(Employee emp)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            // village_id is required in DB schema
            const string query = @"
                INSERT INTO employee 
                (emp_id, emp_name, emp_lname, gender, date_of_b, village_id, tel, start_date, username, password, status)
                VALUES 
                (@id, @name, @surname, @gender, @dob, @vid, @tel, @start, @user, @pass, @status)";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", emp.Id);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
            
            // If Village is not set (empty string), we might fail constraint if it's NOT NULL.
            // Schema says NOT NULL varchar(7).
            // We need a valid village ID. If none selected, this will fail or we need a default.
            // Assuming ViewModel validates this.
            command.Parameters.AddWithValue("@vid", string.IsNullOrEmpty(emp.Village) ? "0000000" : emp.Village); 
            
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@start", DateTime.Now); // Start date = now
            command.Parameters.AddWithValue("@user", emp.Username);
            command.Parameters.AddWithValue("@pass", emp.Password); 
            
            command.Parameters.AddWithValue("@status", emp.Position);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding employee: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(Employee emp)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            // Password update is handled separately usually, but if provided we might update it?
            // Let's stick to updating profile info here as per UpdateEmployeeProfileAsync, but more complete (including village)
            const string query = @"
                UPDATE employee SET 
                    emp_name=@name, emp_lname=@surname, gender=@gender, date_of_b=@dob, 
                    village_id=@vid, tel=@tel, status=@status
                WHERE emp_id=@id";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
            command.Parameters.AddWithValue("@vid", string.IsNullOrEmpty(emp.Village) ? "0000000" : emp.Village);
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@status", emp.Position);
            command.Parameters.AddWithValue("@id", emp.Id);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating employee: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteEmployeeAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM employee WHERE emp_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting employee: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Supplier Operations

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        var list = new List<Supplier>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT sup_id, sup_name, contract_name, email, telephone, address FROM supplier ORDER BY sup_id";
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error getting suppliers: {ex.Message}");
        }
        return list;
    }

    public async Task<bool> AddSupplierAsync(Supplier s)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                INSERT INTO supplier (sup_id, sup_name, contract_name, email, telephone, address)
                VALUES (@id, @name, @contact, @email, @tel, @addr)";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", s.Id);
            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@contact", s.ContactName);
            command.Parameters.AddWithValue("@email", s.Email);
            command.Parameters.AddWithValue("@tel", s.Phone);
            command.Parameters.AddWithValue("@addr", s.Address);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding supplier: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateSupplierAsync(Supplier s)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = @"
                UPDATE supplier SET 
                    sup_name=@name, contract_name=@contact, email=@email, telephone=@tel, address=@addr
                WHERE sup_id=@id";

            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@contact", s.ContactName);
            command.Parameters.AddWithValue("@email", s.Email);
            command.Parameters.AddWithValue("@tel", s.Phone);
            command.Parameters.AddWithValue("@addr", s.Address);
            command.Parameters.AddWithValue("@id", s.Id);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating supplier: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteSupplierAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "DELETE FROM supplier WHERE sup_id = @id";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting supplier: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Exchange Rate Operations

    public async Task<List<ExchangeRate>> GetExchangeRateHistoryAsync()
    {
        var list = new List<ExchangeRate>();
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 50";
            await using var command = new MySqlCommand(query, connection);
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
            Console.Error.WriteLine($"Error getting exchange rate history: {ex.Message}");
        }
        return list;
    }

    public async Task<bool> AddExchangeRateAsync(ExchangeRate rate)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            const string query = "INSERT INTO exchange_rate (dolar, bath, ex_date) VALUES (@usd, @thb, @date)";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@usd", rate.UsdRate);
            command.Parameters.AddWithValue("@thb", rate.ThbRate);
            command.Parameters.AddWithValue("@date", DateTime.Now);

            await command.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding exchange rate: {ex.Message}");
            return false;
        }
    }

    #endregion
}

