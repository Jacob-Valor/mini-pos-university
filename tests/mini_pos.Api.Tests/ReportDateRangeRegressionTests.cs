using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using mini_pos.Api.Controllers;
using mini_pos.Api.DTOs;
using mini_pos.Api.Services;
using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Api.Tests;

public sealed class ReportDateRangeRegressionTests
{
    [Fact]
    public async Task SalesController_GetSalesReport_DefaultsToTodayWindow()
    {
        var fakeSalesRepository = new CapturingSalesRepository();
        var salesService = new SalesApplicationService(fakeSalesRepository);
        var controller = new SalesController(salesService);

        var response = await controller.GetSalesReport(null, null);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.IsType<List<SalesReportItem>>(ok.Value);
        Assert.NotNull(fakeSalesRepository.LastStartDate);
        Assert.NotNull(fakeSalesRepository.LastEndDate);
        Assert.Equal(DateTime.Today, fakeSalesRepository.LastStartDate!.Value.Date);
        Assert.Equal(DateTime.Today, fakeSalesRepository.LastEndDate!.Value.Date);
    }

    [Fact]
    public async Task ReportsController_GetDailySales_UsesSingleDayWindow()
    {
        var fakeSalesRepository = new CapturingSalesRepository
        {
            NextReport =
            [
                new SalesReportItem
                {
                    Total = 100m
                },
                new SalesReportItem
                {
                    Total = 250m
                }
            ]
        };

        var controller = new ReportsController(fakeSalesRepository, new NoopProductRepository());
        var requestedDate = new DateTime(2026, 2, 10, 15, 42, 0, DateTimeKind.Local);

        var response = await controller.GetDailySales(requestedDate);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<DailySalesReportDto>(ok.Value);
        Assert.NotNull(fakeSalesRepository.LastStartDate);
        Assert.NotNull(fakeSalesRepository.LastEndDate);
        Assert.Equal(requestedDate.Date, fakeSalesRepository.LastStartDate!.Value.Date);
        Assert.Equal(requestedDate.Date, fakeSalesRepository.LastEndDate!.Value.Date);
        Assert.Equal(requestedDate.Date, payload.Date.Date);
        Assert.Equal(350m, payload.TotalSales);
        Assert.Equal(2, payload.TransactionCount);
    }

    private sealed class CapturingSalesRepository : ISalesRepository
    {
        public DateTime? LastStartDate { get; private set; }
        public DateTime? LastEndDate { get; private set; }
        public List<SalesReportItem> NextReport { get; set; } = [];

        public Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
            => Task.FromResult(true);

        public Task<List<SalesReportItem>> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            LastStartDate = startDate;
            LastEndDate = endDate;
            return Task.FromResult(NextReport);
        }
    }

    private sealed class NoopProductRepository : IProductRepository
    {
        public Task<List<Product>> GetProductsAsync() => Task.FromResult(new List<Product>());
        public Task<Product?> GetProductByBarcodeAsync(string barcode) => Task.FromResult<Product?>(null);
        public Task<bool> ProductExistsAsync(string barcode) => Task.FromResult(false);
        public Task<bool> AddProductAsync(Product product) => Task.FromResult(true);
        public Task<bool> UpdateProductAsync(Product product) => Task.FromResult(true);
        public Task<bool> DeleteProductAsync(string barcode) => Task.FromResult(true);
    }
}
