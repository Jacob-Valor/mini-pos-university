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
    public async Task GetCustomers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/customers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
}
