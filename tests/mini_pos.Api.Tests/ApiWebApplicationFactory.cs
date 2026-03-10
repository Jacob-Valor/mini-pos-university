using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.Api.Tests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost;Port=3306;Database=mini_pos_test;User=test_user;Password=test_password;SslMode=None;"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IBrandRepository>();
            services.RemoveAll<IProductTypeRepository>();
            services.RemoveAll<IProductRepository>();
            services.RemoveAll<IEmployeeRepository>();
            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<ISalesRepository>();
            services.RemoveAll<ISupplierRepository>();

            services.AddSingleton<IBrandRepository, FakeBrandRepository>();
            services.AddSingleton<IProductTypeRepository, FakeProductTypeRepository>();
            services.AddSingleton<IProductRepository, FakeProductRepository>();
            services.AddSingleton<IEmployeeRepository, FakeEmployeeRepository>();
            services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
            services.AddSingleton<ISalesRepository, FakeSalesRepository>();
            services.AddSingleton<ISupplierRepository, FakeSupplierRepository>();
        });
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly List<Brand> _brands =
        [
            new()
            {
                Id = "B001",
                Name = "Seed Brand"
            }
        ];

        public Task<List<Brand>> GetBrandsAsync()
            => Task.FromResult(_brands.ToList());

        public Task<bool> AddBrandAsync(Brand brand)
        {
            ArgumentNullException.ThrowIfNull(brand);
            if (_brands.Any(x => x.Id == brand.Id))
                return Task.FromResult(false);

            _brands.Add(brand);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateBrandAsync(Brand brand)
        {
            ArgumentNullException.ThrowIfNull(brand);
            var existing = _brands.FirstOrDefault(x => x.Id == brand.Id);
            if (existing is null)
                return Task.FromResult(false);

            existing.Name = brand.Name;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteBrandAsync(string brandId)
        {
            var removed = _brands.RemoveAll(x => x.Id == brandId) > 0;
            return Task.FromResult(removed);
        }
    }

    private sealed class FakeProductTypeRepository : IProductTypeRepository
    {
        private readonly List<ProductType> _types =
        [
            new()
            {
                Id = "C001",
                Name = "Seed Category"
            }
        ];

        public Task<List<ProductType>> GetProductTypesAsync()
            => Task.FromResult(_types.ToList());

        public Task<bool> AddProductTypeAsync(ProductType type)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (_types.Any(x => x.Id == type.Id))
                return Task.FromResult(false);

            _types.Add(type);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProductTypeAsync(ProductType type)
        {
            ArgumentNullException.ThrowIfNull(type);
            var existing = _types.FirstOrDefault(x => x.Id == type.Id);
            if (existing is null)
                return Task.FromResult(false);

            existing.Name = type.Name;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteProductTypeAsync(string typeId)
        {
            var removed = _types.RemoveAll(x => x.Id == typeId) > 0;
            return Task.FromResult(removed);
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly List<Product> _products =
        [
            new()
            {
                Barcode = "P000000000001",
                ProductName = "Seed Product",
                Unit = "pcs",
                Quantity = 100,
                QuantityMin = 5,
                CostPrice = 5000m,
                RetailPrice = 7000m,
                BrandId = "B001",
                BrandName = "Seed Brand",
                CategoryId = "C001",
                CategoryName = "Seed Category",
                Status = "ມີ"
            }
        ];

        public Task<List<Product>> GetProductsAsync()
            => Task.FromResult(_products.ToList());

        public Task<Product?> GetProductByBarcodeAsync(string barcode)
            => Task.FromResult(_products.FirstOrDefault(x => x.Barcode == barcode));

        public Task<bool> ProductExistsAsync(string barcode)
            => Task.FromResult(_products.Any(x => x.Barcode == barcode));

        public Task<bool> AddProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            if (_products.Any(x => x.Barcode == product.Barcode))
                return Task.FromResult(false);

            _products.Add(product);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProductAsync(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            var index = _products.FindIndex(x => x.Barcode == product.Barcode);
            if (index < 0)
                return Task.FromResult(false);

            _products[index] = product;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteProductAsync(string barcode)
        {
            var removed = _products.RemoveAll(x => x.Barcode == barcode) > 0;
            return Task.FromResult(removed);
        }
    }

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employees =
        [
            new()
            {
                Id = "EMP001",
                Name = "Test",
                Surname = "Employee",
                Gender = "M",
                DateOfBirth = new DateTime(2000, 1, 1),
                StartDate = new DateTime(2020, 1, 1),
                VillageId = "010101",
                PhoneNumber = "0200000000",
                Position = "Admin",
                Status = "Admin",
                Username = "admin"
            }
        ];

        public Task<Employee?> GetEmployeeByUsernameAsync(string username)
            => Task.FromResult(_employees.FirstOrDefault(x => x.Username == username));

        public Task<List<Employee>> GetEmployeesAsync()
            => Task.FromResult(_employees.ToList());

        public Task<bool> AddEmployeeAsync(Employee emp)
        {
            ArgumentNullException.ThrowIfNull(emp);
            if (_employees.Any(x => x.Id == emp.Id))
                return Task.FromResult(false);

            _employees.Add(emp);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateEmployeeAsync(Employee emp)
        {
            ArgumentNullException.ThrowIfNull(emp);
            var index = _employees.FindIndex(x => x.Id == emp.Id);
            if (index < 0)
                return Task.FromResult(false);

            _employees[index] = emp;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteEmployeeAsync(string empId)
        {
            var removed = _employees.RemoveAll(x => x.Id == empId) > 0;
            return Task.FromResult(removed);
        }

        public Task<bool> UpdateEmployeeProfileAsync(Employee emp)
            => UpdateEmployeeAsync(emp);
    }

    private sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers =
        [
            new()
            {
                Id = "CUS0000001",
                Name = "Test",
                Surname = "Customer",
                Gender = "M",
                Address = "Somewhere",
                PhoneNumber = "0200000000"
            }
        ];

        public Task<List<Customer>> GetCustomersAsync()
            => Task.FromResult(_customers.ToList());

        public Task<List<Customer>> SearchCustomersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Task.FromResult(_customers.ToList());

            var matches = _customers
                .Where(c =>
                    c.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(matches);
        }

        public Task<bool> AddCustomerAsync(Customer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);
            if (_customers.Any(x => x.Id == customer.Id))
                return Task.FromResult(false);

            _customers.Add(customer);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateCustomerAsync(Customer customer)
        {
            ArgumentNullException.ThrowIfNull(customer);
            var index = _customers.FindIndex(x => x.Id == customer.Id);
            if (index < 0)
                return Task.FromResult(false);

            _customers[index] = customer;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteCustomerAsync(string customerId)
        {
            var removed = _customers.RemoveAll(x => x.Id == customerId) > 0;
            return Task.FromResult(removed);
        }
    }

    private sealed class FakeSalesRepository : ISalesRepository
    {
        private readonly List<SalesReportItem> _reportItems =
        [
            new()
            {
                No = 1,
                Barcode = "P000000000001",
                ProductName = "Seed Product",
                Unit = "pcs",
                Quantity = 1,
                Price = 7000m,
                Total = 7000m
            }
        ];

        public Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
        {
            ArgumentNullException.ThrowIfNull(sale);
            ArgumentNullException.ThrowIfNull(details);

            return Task.FromResult(details.Any());
        }

        public Task<List<SalesReportItem>> GetSalesReportAsync(DateTime startDate, DateTime endDate)
            => Task.FromResult(_reportItems.ToList());
    }

    private sealed class FakeSupplierRepository : ISupplierRepository
    {
        private readonly List<Supplier> _suppliers =
        [
            new()
            {
                Id = "SUP001",
                Name = "Seed Supplier",
                ContactName = "Seed Contact",
                Email = "seed@example.com",
                Phone = "0200000001",
                Address = "Seed Address"
            }
        ];

        public Task<List<Supplier>> GetSuppliersAsync()
            => Task.FromResult(_suppliers.ToList());

        public Task<bool> AddSupplierAsync(Supplier supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);
            if (_suppliers.Any(x => x.Id == supplier.Id))
                return Task.FromResult(false);

            _suppliers.Add(supplier);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);
            var index = _suppliers.FindIndex(x => x.Id == supplier.Id);
            if (index < 0)
                return Task.FromResult(false);

            _suppliers[index] = supplier;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteSupplierAsync(string supplierId)
        {
            var removed = _suppliers.RemoveAll(x => x.Id == supplierId) > 0;
            return Task.FromResult(removed);
        }
    }
}
