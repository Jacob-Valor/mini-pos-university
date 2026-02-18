// Simple database connection test
// Run with: dotnet run --project src/mini_pos.Desktop/mini_pos.csproj -- --test-db

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mini_pos.Services;
using mini_pos.Configuration;
using MySqlConnector;

namespace mini_pos;

public static class DatabaseConnectionTest
{
    public static async Task RunTestAsync()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("Mini POS - Database Connection Test");
        Console.WriteLine("===========================================\n");

        try
        {
            var configuration = BuildConfiguration();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services
                .AddOptions<DatabaseOptions>()
                .Bind(configuration.GetSection(DatabaseOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.DefaultConnection), "ConnectionStrings:DefaultConnection is required")
                .ValidateOnStart();

            services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();
            services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<ICustomerRepository, CustomerRepository>();
            services.AddSingleton<IBrandRepository, BrandRepository>();

            using var provider = services.BuildServiceProvider();
            var connectionFactory = provider.GetRequiredService<IMySqlConnectionFactory>();
            var employeeRepo = provider.GetRequiredService<IEmployeeRepository>();
            var productRepo = provider.GetRequiredService<IProductRepository>();
            var customerRepo = provider.GetRequiredService<ICustomerRepository>();
            var brandRepo = provider.GetRequiredService<IBrandRepository>();

            // Test 1: Connection Test
            Console.WriteLine("[Test 1] Testing database connection...");
            var (success, message) = await TestConnectionAsync(connectionFactory);
            Console.WriteLine($"  Result: {(success ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"  Message: {message}\n");

            if (!success)
            {
                Console.WriteLine("Connection failed. Please ensure:");
                Console.WriteLine("  1. MariaDB container is running: docker-compose up -d mariadb");
                Console.WriteLine("  2. Connection string in appsettings.json is correct");
                return;
            }

            // Test 2: Get Employees
            Console.WriteLine("[Test 2] Fetching employees...");
            var employees = await employeeRepo.GetEmployeesAsync();
            Console.WriteLine($"  Found {employees.Count} employee(s)");
            foreach (var emp in employees.Take(3))
            {
                Console.WriteLine($"    - {emp.Id}: {emp.Name} {emp.Surname} ({emp.Position})");
            }
            Console.WriteLine();

            // Test 3: Get Products
            Console.WriteLine("[Test 3] Fetching products...");
            var products = await productRepo.GetProductsAsync();
            Console.WriteLine($"  Found {products.Count} product(s)");
            foreach (var prod in products.Take(3))
            {
                Console.WriteLine($"    - {prod.Barcode}: {prod.ProductName} - {prod.RetailPrice:N0} LAK");
            }
            Console.WriteLine();

            // Test 4: Get Customers
            Console.WriteLine("[Test 4] Fetching customers...");
            var customers = await customerRepo.GetCustomersAsync();
            Console.WriteLine($"  Found {customers.Count} customer(s)");
            foreach (var cust in customers.Take(3))
            {
                Console.WriteLine($"    - {cust.Id}: {cust.Name} {cust.Surname}");
            }
            Console.WriteLine();

            // Test 5: Get Brands
            Console.WriteLine("[Test 5] Fetching brands...");
            var brands = await brandRepo.GetBrandsAsync();
            Console.WriteLine($"  Found {brands.Count} brand(s)");
            foreach (var brand in brands.Take(5))
            {
                Console.WriteLine($"    - {brand.Id}: {brand.Name}");
            }
            Console.WriteLine();

            Console.WriteLine("===========================================");
            Console.WriteLine("All tests completed successfully!");
            Console.WriteLine("===========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var baseDir = AppContext.BaseDirectory;

        if (!File.Exists(Path.Combine(baseDir, "appsettings.json")))
        {
            var currentDir = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
                baseDir = currentDir;
        }

        DotEnvLoader.LoadFromSearchPaths(baseDir, Directory.GetCurrentDirectory(), AppContext.BaseDirectory);

        return new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static async Task<(bool Success, string Message)> TestConnectionAsync(IMySqlConnectionFactory connectionFactory)
    {
        try
        {
            await using var connection = await connectionFactory.OpenConnectionAsync();
            await using var command = new MySqlCommand(SqlQueries.ConnectionTest, connection);
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
}
