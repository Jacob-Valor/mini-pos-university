using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using Xunit;

namespace mini_pos.Api.Tests;

public sealed class ApiEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiEndpointTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBrands_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/brands");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductTypes_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/producttypes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_ReturnsActualStatusAndStartDateFromRepository()
    {
        var employees = await _client.GetFromJsonAsync<List<EmployeeResponse>>("/api/employees");
        Assert.NotNull(employees);

        var employee = Assert.Single(employees!);
        Assert.Equal("EMP001", employee.EmpId);
        Assert.Equal("Admin", employee.Position);
        Assert.Equal("Admin", employee.Status);
        Assert.Equal(new DateTime(2020, 1, 1), employee.StartDate.Date);
    }

    [Fact]
    public async Task GetEmployeeById_WhenEmployeeExists_ReturnsOkWithPayload()
    {
        var employee = await _client.GetFromJsonAsync<EmployeeResponse>("/api/employees/EMP001");

        Assert.NotNull(employee);
        Assert.Equal("EMP001", employee!.EmpId);
        Assert.Equal("Admin", employee.Status);
    }

    [Fact]
    public async Task GetEmployeeById_WhenEmployeeMissing_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/employees/EMP404");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSuppliers_ReturnsMappedSupplierData()
    {
        var suppliers = await _client.GetFromJsonAsync<List<SupplierResponse>>("/api/suppliers");

        var supplier = Assert.Single(suppliers!);
        Assert.Equal("SUP001", supplier.SupId);
        Assert.Equal("Seed Contact", supplier.ContractName);
        Assert.Equal("seed@example.com", supplier.Email);
    }

    [Fact]
    public async Task CreateBrand_WithExistingId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/brands", new
        {
            Id = "B001",
            Name = "Duplicate Brand"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBrand_WithNewId_ReturnsCreatedAndPersistsBrand()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/brands", new
        {
            Id = "B999",
            Name = "New Brand"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var brands = await _client.GetFromJsonAsync<List<BrandResponse>>("/api/brands");
        Assert.Contains(brands!, brand => brand.Id == "B999" && brand.Name == "New Brand");
    }

    [Fact]
    public async Task GetSalesReport_WithValidDateRange_ReturnsSeedReportItem()
    {
        var reportItems = await _client.GetFromJsonAsync<List<SalesReportResponse>>("/api/sales/report?startDate=2026-02-09&endDate=2026-02-10");

        var reportItem = Assert.Single(reportItems!);
        Assert.Equal("P000000000001", reportItem.Barcode);
        Assert.Equal(7000m, reportItem.Total);
    }

    [Fact]
    public async Task GetSalesReport_WithInvalidDateRange_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/sales/report?startDate=2026-02-10&endDate=2026-02-09");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSale_WithEmptyItems_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new
        {
            ExId = 1,
            CusId = "CUS0000001",
            EmpId = "EMP001",
            Subtotal = 7000m,
            Pay = 7000m,
            MoneyChange = 0m,
            Items = Array.Empty<object>()
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSale_WithInvalidItem_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new
        {
            ExId = 1,
            CusId = "CUS0000001",
            EmpId = "EMP001",
            Subtotal = 7000m,
            Pay = 7000m,
            MoneyChange = 0m,
            Items = new[]
            {
                new
                {
                    ProductId = "P000000000001",
                    Qty = 0,
                    Price = 7000m,
                    Total = 7000m
                }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateSale_WithValidPayload_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", new
        {
            ExId = 1,
            CusId = "CUS0000001",
            EmpId = "EMP001",
            Subtotal = 7000m,
            Pay = 10000m,
            MoneyChange = 3000m,
            Items = new[]
            {
                new
                {
                    ProductId = "P000000000001",
                    Qty = 1,
                    Price = 7000m,
                    Total = 7000m
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record EmployeeResponse(
        string EmpId,
        DateTime StartDate,
        string Position,
        string Status
    );

    private sealed record SupplierResponse(
        string SupId,
        string ContractName,
        string Email
    );

    private sealed record BrandResponse(
        string Id,
        string Name
    );

    private sealed record SalesReportResponse(
        string Barcode,
        decimal Total
    );
}
