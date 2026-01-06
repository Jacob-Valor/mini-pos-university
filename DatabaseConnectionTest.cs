// Simple database connection test
// Run with: dotnet run --project mini_pos.csproj -- --test-db

using System;
using System.Linq;
using System.Threading.Tasks;
using mini_pos.Services;

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
            // Test 1: Connection Test
            Console.WriteLine("[Test 1] Testing database connection...");
            var (success, message) = await DatabaseService.Instance.TestConnectionAsync();
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
            var employees = await DatabaseService.Instance.GetEmployeesAsync();
            Console.WriteLine($"  Found {employees.Count} employee(s)");
            foreach (var emp in employees.Take(3))
            {
                Console.WriteLine($"    - {emp.Id}: {emp.Name} {emp.Surname} ({emp.Position})");
            }
            Console.WriteLine();

            // Test 3: Get Products
            Console.WriteLine("[Test 3] Fetching products...");
            var products = await DatabaseService.Instance.GetProductsAsync();
            Console.WriteLine($"  Found {products.Count} product(s)");
            foreach (var prod in products.Take(3))
            {
                Console.WriteLine($"    - {prod.Barcode}: {prod.ProductName} - {prod.RetailPrice:N0} LAK");
            }
            Console.WriteLine();

            // Test 4: Get Customers
            Console.WriteLine("[Test 4] Fetching customers...");
            var customers = await DatabaseService.Instance.GetCustomersAsync();
            Console.WriteLine($"  Found {customers.Count} customer(s)");
            foreach (var cust in customers.Take(3))
            {
                Console.WriteLine($"    - {cust.Id}: {cust.Name} {cust.Surname}");
            }
            Console.WriteLine();

            // Test 5: Get Brands
            Console.WriteLine("[Test 5] Fetching brands...");
            var brands = await DatabaseService.Instance.GetBrandsAsync();
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
}
