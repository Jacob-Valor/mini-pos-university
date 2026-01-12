using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using mini_pos.Models;
using MySqlConnector;
using Serilog;

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
    Task<bool> ProductExistsAsync(string barcode);

    // Customer Operations
    Task<bool> AddCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(string customerId);
    Task<List<Customer>> SearchCustomersAsync(string keyword);
    
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
    private static class SqlQueries
    {
        public const string ConnectionTest = "SELECT 1";

        public const string ValidateLogin = @"
                SELECT emp_id, emp_name, emp_lname, gender, date_of_b, 
                       village_id, tel, start_date, username, status
                FROM employee 
                WHERE username = @username AND password = @password";

        public const string Employees = @"
                SELECT e.emp_id, e.emp_name, e.emp_lname, e.gender, e.date_of_b,
                       e.tel, e.start_date, e.username, e.status,
                       v.vname as village_name, d.distname as district_name, p.provname as province_name
                FROM employee e
                LEFT JOIN villages v ON e.village_id = v.vid
                LEFT JOIN districts d ON v.distid = d.distid
                LEFT JOIN provinces p ON d.provid = p.provid
                ORDER BY e.emp_id";

        public const string Products = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       p.brand_id, p.category_id,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                ORDER BY p.barcode";

        public const string ProductByBarcode = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       p.brand_id, p.category_id,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                WHERE p.barcode = @barcode";

        public const string Customers = @"
                SELECT cus_id, cus_name, cus_lname, gender, address, tel
                FROM customer
                ORDER BY cus_id";

        public const string Brands = "SELECT brand_id, brand_name FROM brand ORDER BY brand_id";
        public const string ProductTypes = "SELECT category_id, category_name FROM category";

        public const string LatestExchangeRate = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 1";
        public const string ExchangeRateHistory = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 50";

        public const string ProductExists = "SELECT COUNT(1) FROM product WHERE barcode = @id";
        public const string InsertProduct = @"
                INSERT INTO product (barcode, product_name, unit, quantity, quantity_min, cost_price, retail_price, brand_id, category_id, status)
                VALUES (@id, @name, @unit, @qty, @min, @cost, @price, @brand, @type, @status)";
        public const string UpdateProduct = @"
                UPDATE product SET 
                    product_name=@name, unit=@unit, quantity=@qty, quantity_min=@min, 
                    cost_price=@cost, retail_price=@price, brand_id=@brand, category_id=@type, status=@status
                WHERE barcode=@id";
        public const string DeleteProduct = "DELETE FROM product WHERE barcode = @id";

        public const string InsertCustomer = @"
                INSERT INTO customer (cus_id, cus_name, cus_lname, gender, address, tel)
                VALUES (@id, @name, @surname, @gender, @addr, @tel)";
        public const string UpdateCustomer = @"
                UPDATE customer SET 
                    cus_name=@name, cus_lname=@surname, gender=@gender, address=@addr, tel=@tel
                WHERE cus_id=@id";
        public const string DeleteCustomer = "DELETE FROM customer WHERE cus_id = @id";
        public const string SearchCustomers = @"
                SELECT cus_id, cus_name, cus_lname, gender, address, tel 
                FROM customer 
                WHERE cus_id LIKE @kw OR cus_name LIKE @kw OR tel LIKE @kw
                LIMIT 20";

        public const string InsertBrand = "INSERT INTO brand (brand_id, brand_name) VALUES (@id, @name)";
        public const string UpdateBrand = "UPDATE brand SET brand_name = @name WHERE brand_id = @id";
        public const string DeleteBrand = "DELETE FROM brand WHERE brand_id = @id";

        public const string InsertProductType = "INSERT INTO category (category_id, category_name) VALUES (@id, @name)";
        public const string UpdateProductType = "UPDATE category SET category_name = @name WHERE category_id = @id";
        public const string DeleteProductType = "DELETE FROM category WHERE category_id = @id";

        public const string Provinces = "SELECT provid, provname FROM provinces ORDER BY provname";
        public const string DistrictsByProvince = "SELECT distid, distname, provid FROM districts WHERE provid = @pid ORDER BY distname";
        public const string VillagesByDistrict = "SELECT vid, vname, distid FROM villages WHERE distid = @did ORDER BY vname";

        public const string InsertEmployee = @"
                INSERT INTO employee 
                (emp_id, emp_name, emp_lname, gender, date_of_b, village_id, tel, start_date, username, password, status)
                VALUES 
                (@id, @name, @surname, @gender, @dob, @vid, @tel, @start, @user, @pass, @status)";
        public const string UpdateEmployee = @"
                UPDATE employee SET 
                    emp_name=@name, emp_lname=@surname, gender=@gender, date_of_b=@dob, 
                    village_id=@vid, tel=@tel, status=@status
                WHERE emp_id=@id";
        public const string DeleteEmployee = "DELETE FROM employee WHERE emp_id = @id";
        public const string UpdateEmployeeProfile = @"
                UPDATE employee 
                SET emp_name = @name, 
                    emp_lname = @surname, 
                    gender = @gender, 
                    date_of_b = @dob, 
                    tel = @tel,
                    username = @username
                WHERE emp_id = @id";
        public const string UpdateEmployeePassword = "UPDATE employee SET password = @pwd WHERE emp_id = @id";
        public const string StoredPasswordHash = "SELECT password FROM employee WHERE username = @username";

        public const string Suppliers = "SELECT sup_id, sup_name, contract_name, email, telephone, address FROM supplier ORDER BY sup_id";
        public const string InsertSupplier = @"
                INSERT INTO supplier (sup_id, sup_name, contract_name, email, telephone, address)
                VALUES (@id, @name, @contact, @email, @tel, @addr)";
        public const string UpdateSupplier = @"
                UPDATE supplier SET 
                    sup_name=@name, contract_name=@contact, email=@email, telephone=@tel, address=@addr
                WHERE sup_id=@id";
        public const string DeleteSupplier = "DELETE FROM supplier WHERE sup_id = @id";

        public const string InsertExchangeRate = "INSERT INTO exchange_rate (dolar, bath, ex_date) VALUES (@usd, @thb, @date)";

        public const string InsertSale = @"
                INSERT INTO sales (ex_id, cus_id, emp_id, date_sale, subtotal, pay, money_change)
                VALUES (@exId, @cusId, @empId, @date, @sub, @pay, @change);
                SELECT LAST_INSERT_ID();";
        public const string InsertSaleDetail = @"
                INSERT INTO sales_product (sales_id, product_id, qty, price, total)
                VALUES (@saleId, @prodId, @qty, @price, @total)";
        public const string UpdateStock = @"
                UPDATE product SET quantity = quantity - @qty WHERE barcode = @prodId";
    }

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
            await using var command = new MySqlCommand(SqlQueries.ConnectionTest, connection);
            await command.ExecuteScalarAsync();
            Log.Information("Database connection test successful");
            return (true, "ເຊື່ອມຕໍ່ຖານຂໍ້ມູນສຳເລັດ (Database connection successful)");
        }
        catch (MySqlException ex)
        {
            Log.Error(ex, "Database connection failed");
            return (false, $"ເຊື່ອມຕໍ່ຖານຂໍ້ມູນລົ້ມເຫຼວ: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error during connection test");
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
            await using var command = new MySqlCommand(SqlQueries.ValidateLogin, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", passwordHash);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Log.Information("User {Username} logged in successfully", username);
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
            Log.Warning("Failed login attempt for user: {Username}", username);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error validating login for user: {Username}", username);
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
            await using var command = new MySqlCommand(SqlQueries.Products, connection);
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
                    BrandId = reader.IsDBNull("brand_id") ? "" : reader.GetString("brand_id"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryId = reader.IsDBNull("category_id") ? "" : reader.GetString("category_id"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching products");
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
            await using var command = new MySqlCommand(SqlQueries.ProductByBarcode, connection);
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
                    BrandId = reader.IsDBNull("brand_id") ? "" : reader.GetString("brand_id"),
                    BrandName = reader.IsDBNull("brand_name") ? "" : reader.GetString("brand_name"),
                    CategoryId = reader.IsDBNull("category_id") ? "" : reader.GetString("category_id"),
                    CategoryName = reader.IsDBNull("category_name") ? "" : reader.GetString("category_name"),
                    Status = reader.GetString("status")
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error finding product by barcode: {Barcode}", barcode);
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
            Log.Error(ex, "Error executing non-query: {Query}", query);
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
            Log.Error(ex, "Error executing scalar query: {Query}", query);
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
            await using var command = new MySqlCommand(SqlQueries.UpdateEmployeeProfile, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
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

    /// <summary>
    /// Updates employee password.
    /// </summary>
    public async Task<bool> UpdatePasswordAsync(string empId, string newPasswordHash)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateEmployeePassword, connection);
            command.Parameters.AddWithValue("@pwd", newPasswordHash);
            command.Parameters.AddWithValue("@id", empId);

            int rows = await command.ExecuteNonQueryAsync();
            Log.Information("Password updated for employee {Id}", empId);
            return rows > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating password for employee {Id}", empId);
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
            await using var command = new MySqlCommand(SqlQueries.StoredPasswordHash, connection);
            command.Parameters.AddWithValue("@username", username);
            
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching password hash for user {Username}", username);
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

    public async Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
    {
        MySqlTransaction? transaction = null;
        try
        {
            await using var connection = await GetConnectionAsync();
            transaction = await connection.BeginTransactionAsync();

            // 1. Insert Sales Header
            await using var saleCmd = new MySqlCommand(SqlQueries.InsertSale, connection, transaction);
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
            foreach (var item in details)
            {
                // Detail Insert
                await using var detailCmd = new MySqlCommand(SqlQueries.InsertSaleDetail, connection, transaction);
                detailCmd.Parameters.AddWithValue("@saleId", saleId);
                detailCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                detailCmd.Parameters.AddWithValue("@qty", item.Quantity);
                detailCmd.Parameters.AddWithValue("@price", item.Price);
                detailCmd.Parameters.AddWithValue("@total", item.Total);
                await detailCmd.ExecuteNonQueryAsync();

                // Stock Update
                await using var stockCmd = new MySqlCommand(SqlQueries.UpdateStock, connection, transaction);
                stockCmd.Parameters.AddWithValue("@qty", item.Quantity);
                stockCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                await stockCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            Log.Information("Sale created successfully. ID: {SaleId}, Amount: {Amount}", saleId, sale.SubTotal);
            return true;
        }
        catch (Exception ex)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync();
            }
            Log.Error(ex, "Error creating sale");
            return false;
        }
    }

    #endregion

    #region Product Operations

    public async Task<bool> ProductExistsAsync(string barcode)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ProductExists, connection);
            command.Parameters.AddWithValue("@id", barcode);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking product existence: {Barcode}", barcode);
            return false;
        }
    }

    public async Task<bool> AddProductAsync(Product p)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertProduct, connection);
            
            command.Parameters.AddWithValue("@id", p.Id);
            command.Parameters.AddWithValue("@name", p.Name);
            command.Parameters.AddWithValue("@unit", p.Unit);
            command.Parameters.AddWithValue("@qty", p.Quantity);
            command.Parameters.AddWithValue("@min", p.MinQuantity);
            command.Parameters.AddWithValue("@cost", p.CostPrice);
            command.Parameters.AddWithValue("@price", p.SellingPrice);
            command.Parameters.AddWithValue("@brand", p.BrandId); 
            command.Parameters.AddWithValue("@type", p.CategoryId);
            command.Parameters.AddWithValue("@status", p.Status);

            await command.ExecuteNonQueryAsync();
            Log.Information("Product added: {ProductName} ({Barcode})", p.Name, p.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding product: {ProductName}", p.Name);
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(Product p)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateProduct, connection);
            command.Parameters.AddWithValue("@id", p.Id);
            command.Parameters.AddWithValue("@name", p.Name);
            command.Parameters.AddWithValue("@unit", p.Unit);
            command.Parameters.AddWithValue("@qty", p.Quantity);
            command.Parameters.AddWithValue("@min", p.MinQuantity);
            command.Parameters.AddWithValue("@cost", p.CostPrice);
            command.Parameters.AddWithValue("@price", p.SellingPrice);
            command.Parameters.AddWithValue("@brand", p.BrandId);
            command.Parameters.AddWithValue("@type", p.CategoryId);
            command.Parameters.AddWithValue("@status", p.Status);

            await command.ExecuteNonQueryAsync();
            Log.Information("Product updated: {ProductName} ({Barcode})", p.Name, p.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product: {Barcode}", p.Id);
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(string barcode)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteProduct, connection);
            command.Parameters.AddWithValue("@id", barcode);
            await command.ExecuteNonQueryAsync();
            Log.Information("Product deleted: {Barcode}", barcode);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product: {Barcode}", barcode);
            return false;
        }
    }

    public async Task<List<ProductType>> GetProductTypesAsync()
    {
         var list = new List<ProductType>();
        try
        {
            await using var connection = await GetConnectionAsync();
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

    #endregion

    #region Customer Operations

    public async Task<bool> AddCustomerAsync(Customer c)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertCustomer, connection);
            command.Parameters.AddWithValue("@id", c.Id);
            command.Parameters.AddWithValue("@name", c.Name);
            command.Parameters.AddWithValue("@surname", c.Surname);
            command.Parameters.AddWithValue("@gender", c.Gender);
            command.Parameters.AddWithValue("@addr", c.Address);
            command.Parameters.AddWithValue("@tel", c.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            Log.Information("Customer added: {Name} {Surname}", c.Name, c.Surname);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding customer");
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(Customer c)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateCustomer, connection);
            command.Parameters.AddWithValue("@id", c.Id);
            command.Parameters.AddWithValue("@name", c.Name);
            command.Parameters.AddWithValue("@surname", c.Surname);
            command.Parameters.AddWithValue("@gender", c.Gender);
            command.Parameters.AddWithValue("@addr", c.Address);
            command.Parameters.AddWithValue("@tel", c.PhoneNumber);

            await command.ExecuteNonQueryAsync();
            Log.Information("Customer updated: {Id}", c.Id);
            return true;
        }
        catch (Exception ex)
        {
             Log.Error(ex, "Error updating customer: {Id}", c.Id);
             return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteCustomer, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            Log.Information("Customer deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
             Log.Error(ex, "Error deleting customer: {Id}", id);
             return false;
        }
    }

    public async Task<List<Customer>> SearchCustomersAsync(string keyword)
    {
        var list = new List<Customer>();
        try
        {
            await using var connection = await GetConnectionAsync();
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

    #endregion

    #region Brand Operations

    public async Task<bool> AddBrandAsync(Brand brand)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
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
            await using var connection = await GetConnectionAsync();
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

    public async Task<bool> DeleteBrandAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteBrand, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            Log.Information("Brand deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting brand: {Id}", id);
            return false;
        }
    }

    #endregion

    #region Type Operations
    
    public async Task<bool> AddProductTypeAsync(ProductType type)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertProductType, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);
            await command.ExecuteNonQueryAsync();
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
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateProductType, connection);
            command.Parameters.AddWithValue("@id", type.Id);
            command.Parameters.AddWithValue("@name", type.Name);
            await command.ExecuteNonQueryAsync();
            Log.Information("Product type updated: {Id}", type.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating product type: {Id}", type.Id);
            return false;
        }
    }

    public async Task<bool> DeleteProductTypeAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteProductType, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            Log.Information("Product type deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting product type: {Id}", id);
            return false;
        }
    }
    
    #endregion


    #region Geo Operations

    public async Task<List<Province>> GetProvincesAsync()
    {
        var list = new List<Province>();
        try
        {
            await using var connection = await GetConnectionAsync();
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
            await using var connection = await GetConnectionAsync();
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
            await using var connection = await GetConnectionAsync();
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

    #endregion

    #region Employee Management (CRUD)

    public async Task<bool> AddEmployeeAsync(Employee emp)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            // village_id is required in DB schema
            await using var command = new MySqlCommand(SqlQueries.InsertEmployee, connection);
            command.Parameters.AddWithValue("@id", emp.Id);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
            command.Parameters.AddWithValue("@vid", string.IsNullOrEmpty(emp.Village) ? "0000000" : emp.Village); 
            command.Parameters.AddWithValue("@tel", emp.PhoneNumber);
            command.Parameters.AddWithValue("@start", DateTime.Now); // Start date = now
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
            await using var connection = await GetConnectionAsync();
            // Password update is handled separately usually, but if provided we might update it?
            // Let's stick to updating profile info here as per UpdateEmployeeProfileAsync, but more complete (including village)
            await using var command = new MySqlCommand(SqlQueries.UpdateEmployee, connection);
            command.Parameters.AddWithValue("@name", emp.Name);
            command.Parameters.AddWithValue("@surname", emp.Surname);
            command.Parameters.AddWithValue("@gender", emp.Gender);
            command.Parameters.AddWithValue("@dob", emp.DateOfBirth);
            command.Parameters.AddWithValue("@vid", string.IsNullOrEmpty(emp.Village) ? "0000000" : emp.Village);
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

    public async Task<bool> DeleteEmployeeAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteEmployee, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            Log.Information("Employee deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting employee: {Id}", id);
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

    public async Task<bool> AddSupplierAsync(Supplier s)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertSupplier, connection);
            command.Parameters.AddWithValue("@id", s.Id);
            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@contact", s.ContactName);
            command.Parameters.AddWithValue("@email", s.Email);
            command.Parameters.AddWithValue("@tel", s.Phone);
            command.Parameters.AddWithValue("@addr", s.Address);

            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier added: {Name}", s.Name);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error adding supplier: {Name}", s.Name);
            return false;
        }
    }

    public async Task<bool> UpdateSupplierAsync(Supplier s)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.UpdateSupplier, connection);
            command.Parameters.AddWithValue("@name", s.Name);
            command.Parameters.AddWithValue("@contact", s.ContactName);
            command.Parameters.AddWithValue("@email", s.Email);
            command.Parameters.AddWithValue("@tel", s.Phone);
            command.Parameters.AddWithValue("@addr", s.Address);
            command.Parameters.AddWithValue("@id", s.Id);

            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier updated: {Id}", s.Id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating supplier: {Id}", s.Id);
            return false;
        }
    }

    public async Task<bool> DeleteSupplierAsync(string id)
    {
        try
        {
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.DeleteSupplier, connection);
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
            Log.Information("Supplier deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting supplier: {Id}", id);
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
            await using var connection = await GetConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.InsertExchangeRate, connection);
            command.Parameters.AddWithValue("@usd", rate.UsdRate);
            command.Parameters.AddWithValue("@thb", rate.ThbRate);
            command.Parameters.AddWithValue("@date", DateTime.Now);

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

    #endregion
}

