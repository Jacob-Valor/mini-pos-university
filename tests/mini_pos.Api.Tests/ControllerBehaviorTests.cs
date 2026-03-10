using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using mini_pos.Api.Controllers;
using mini_pos.Api.DTOs;
using mini_pos.Api.Services;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using Xunit;

namespace mini_pos.Api.Tests;

public class ControllerBehaviorTests
{
    [Fact]
    public async Task GetSalesReport_WhenServiceFails_ReturnsBadRequest()
    {
        var salesService = new Mock<ISalesApplicationService>();
        salesService
            .Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new SalesReportUseCaseResult(false, new List<SalesReportItem>(), "Invalid date range"));

        var controller = new SalesController(salesService.Object);

        var response = await controller.GetSalesReport(DateTime.Today, DateTime.Today.AddDays(-1));

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Contains("Invalid date range", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task CreateSale_WhenServiceFails_ReturnsBadRequest()
    {
        var salesService = new Mock<ISalesApplicationService>();
        salesService
            .Setup(x => x.CreateSaleAsync(It.IsAny<CreateSaleDto>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new CreateSaleUseCaseResult(false, "Sale items are required"));

        var controller = new SalesController(salesService.Object);

        var response = await controller.CreateSale(new CreateSaleDto(
            1,
            "CUS0000001",
            "EMP001",
            0m,
            0m,
            0m,
            new List<CreateSaleItemDto>()));

        var badRequest = Assert.IsType<BadRequestObjectResult>(response);
        Assert.Contains("Sale items are required", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task CreateSale_WhenServiceSucceeds_ReturnsOk()
    {
        var salesService = new Mock<ISalesApplicationService>();
        salesService
            .Setup(x => x.CreateSaleAsync(It.IsAny<CreateSaleDto>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new CreateSaleUseCaseResult(true));

        var controller = new SalesController(salesService.Object);

        var response = await controller.CreateSale(new CreateSaleDto(
            1,
            "CUS0000001",
            "EMP001",
            100m,
            100m,
            0m,
            [new CreateSaleItemDto("1234567890123", 1, 100m, 100m)]));

        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Contains("Sale created successfully", ok.Value!.ToString());
    }

    [Fact]
    public async Task CreateProduct_MapsBrandAndCategoryNamesIntoResponse()
    {
        var productRepository = new Mock<IProductRepository>();
        Product? createdProduct = null;
        productRepository
            .Setup(x => x.AddProductAsync(It.IsAny<Product>()))
            .Callback<Product>(product => createdProduct = product)
            .ReturnsAsync(true);

        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.GetBrandsAsync()).ReturnsAsync(
            [new Brand { Id = "B001", Name = "Test Brand" }]);

        var productTypeRepository = new Mock<IProductTypeRepository>();
        productTypeRepository.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(
            [new ProductType { Id = "C001", Name = "Test Category" }]);

        var controller = new ProductsController(
            productRepository.Object,
            brandRepository.Object,
            productTypeRepository.Object);

        var response = await controller.CreateProduct(new CreateProductDto(
            "1234567890123",
            "Rice",
            "bag",
            10,
            2,
            100m,
            120m,
            "B001",
            "C001",
            "ມີ"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<ProductDto>(created.Value);

        Assert.NotNull(createdProduct);
        Assert.Equal("Test Brand", createdProduct!.BrandName);
        Assert.Equal("Test Category", createdProduct.CategoryName);
        Assert.Equal("Test Brand", payload.BrandName);
        Assert.Equal("Test Category", payload.CategoryName);
    }

    [Fact]
    public async Task UpdateProduct_WhenRepositoryReturnsFalse_ReturnsNotFound()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(false);

        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());

        var productTypeRepository = new Mock<IProductTypeRepository>();
        productTypeRepository.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());

        var controller = new ProductsController(
            productRepository.Object,
            brandRepository.Object,
            productTypeRepository.Object);

        var response = await controller.UpdateProduct("missing", new UpdateProductDto(
            "Rice",
            "bag",
            10,
            2,
            100m,
            120m,
            "B001",
            "C001",
            "ມີ"));

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateCustomer_GeneratesCustomerIdAndReturnsCreatedPayload()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        Customer? createdCustomer = null;
        customerRepository
            .Setup(x => x.AddCustomerAsync(It.IsAny<Customer>()))
            .Callback<Customer>(customer => createdCustomer = customer)
            .ReturnsAsync(true);

        var controller = new CustomersController(customerRepository.Object);

        var response = await controller.CreateCustomer(new CreateCustomerDto(
            "Jane",
            "Doe",
            "ຍິງ",
            "Vientiane",
            "02012345678"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<CustomerDto>(created.Value);

        Assert.NotNull(createdCustomer);
        Assert.StartsWith("CUS", createdCustomer!.Id);
        Assert.Equal(createdCustomer.Id, payload.CusId);
        Assert.Equal("Jane", payload.CusName);
    }

    [Fact]
    public async Task GetCustomer_WhenMissing_ReturnsNotFound()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer>());

        var controller = new CustomersController(customerRepository.Object);

        var response = await controller.GetCustomer("CUS0000001");

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetEmployee_WhenMissing_ReturnsNotFound()
    {
        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(new List<Employee>());

        var controller = new EmployeesController(employeeRepository.Object);

        var response = await controller.GetEmployee("EMP404");

        var notFound = Assert.IsType<NotFoundObjectResult>(response.Result);
        Assert.Contains("EMP404", notFound.Value!.ToString());
    }

    [Fact]
    public async Task GetEmployee_WhenPositionMissing_FallsBackToStatus()
    {
        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(
        [
            new Employee
            {
                Id = "EMP001",
                Name = "Jane",
                Surname = "Doe",
                Gender = "F",
                DateOfBirth = new DateTime(1995, 1, 1),
                VillageId = "010101",
                PhoneNumber = "02012345678",
                StartDate = new DateTime(2025, 1, 1),
                Position = string.Empty,
                Status = "Manager"
            }
        ]);

        var controller = new EmployeesController(employeeRepository.Object);

        var response = await controller.GetEmployee("EMP001");

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<EmployeeDto>(ok.Value);
        Assert.Equal("Manager", payload.Position);
        Assert.Equal("Manager", payload.Status);
    }

    [Fact]
    public async Task GetLatestExchangeRate_WhenMissing_ReturnsNotFound()
    {
        var exchangeRateRepository = new Mock<IExchangeRateRepository>();
        exchangeRateRepository.Setup(x => x.GetLatestExchangeRateAsync()).ReturnsAsync((ExchangeRate?)null);

        var controller = new ExchangeRatesController(exchangeRateRepository.Object);

        var response = await controller.GetLatest();

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetLowStock_ReturnsOnlyProductsAtOrBelowMinimum()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(
        [
            new Product { Barcode = "LOW1", ProductName = "Low Stock", Quantity = 2, QuantityMin = 5 },
            new Product { Barcode = "OK1", ProductName = "Healthy Stock", Quantity = 6, QuantityMin = 5 },
            new Product { Barcode = "EDGE1", ProductName = "At Minimum", Quantity = 5, QuantityMin = 5 }
        ]);

        var controller = new ReportsController(Mock.Of<ISalesRepository>(), productRepository.Object);

        var response = await controller.GetLowStock();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<LowStockReportDto>>(ok.Value);

        Assert.Collection(
            payload,
            item => Assert.Equal("LOW1", item.Barcode),
            item => Assert.Equal("EDGE1", item.Barcode));
    }

    [Fact]
    public async Task GetSuppliers_WhenRepositoryReturnsEmptyList_ReturnsOkEmptyPayload()
    {
        var supplierRepository = new Mock<ISupplierRepository>();
        supplierRepository.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier>());

        var controller = new SuppliersController(supplierRepository.Object);

        var response = await controller.GetSuppliers();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<SupplierDto>>(ok.Value);
        Assert.Empty(payload);
    }

    [Fact]
    public async Task GetSuppliers_MapsRepositoryFieldsToDto()
    {
        var supplierRepository = new Mock<ISupplierRepository>();
        supplierRepository.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(
        [
            new Supplier
            {
                Id = "SUP001",
                Name = "Rice Supplier",
                ContactName = "Mr. Contact",
                Email = "supplier@example.com",
                Phone = "02012345678",
                Address = "Vientiane"
            }
        ]);

        var controller = new SuppliersController(supplierRepository.Object);

        var response = await controller.GetSuppliers();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<SupplierDto>>(ok.Value);
        var supplier = Assert.Single(payload);
        Assert.Equal("SUP001", supplier.SupId);
        Assert.Equal("Mr. Contact", supplier.ContractName);
    }

    [Fact]
    public async Task CreateBrand_WhenRepositoryReturnsFalse_ReturnsBadRequest()
    {
        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.AddBrandAsync(It.IsAny<Brand>())).ReturnsAsync(false);

        var controller = new BrandsController(brandRepository.Object);

        var response = await controller.CreateBrand(new CreateBrandDto("B001", "New Brand"));

        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.Contains("Failed to create brand", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task CreateBrand_WhenRepositorySucceeds_ReturnsCreatedPayload()
    {
        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.AddBrandAsync(It.IsAny<Brand>())).ReturnsAsync(true);

        var controller = new BrandsController(brandRepository.Object);

        var response = await controller.CreateBrand(new CreateBrandDto("B001", "New Brand"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<BrandDto>(created.Value);
        Assert.Equal("B001", payload.Id);
        Assert.Equal("New Brand", payload.Name);
    }
}
