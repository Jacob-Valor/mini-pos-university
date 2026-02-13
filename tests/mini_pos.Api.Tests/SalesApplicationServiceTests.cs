using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using mini_pos.Api.DTOs;
using mini_pos.Api.Services;
using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Api.Tests;

public sealed class SalesApplicationServiceTests
{
    [Fact]
    public async Task GetSalesReportAsync_WithInvalidDateRange_ReturnsFailureWithoutRepositoryCall()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesApplicationService(repository);

        var result = await service.GetSalesReportAsync(
            new DateTime(2026, 2, 10),
            new DateTime(2026, 2, 9));

        Assert.False(result.IsSuccess);
        Assert.Equal("endDate must be greater than or equal to startDate", result.ErrorMessage);
        Assert.Empty(result.Items);
        Assert.Equal(0, repository.GetSalesReportCallCount);
    }

    [Fact]
    public async Task CreateSaleAsync_WithEmptyItems_ReturnsFailureWithoutRepositoryCall()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesApplicationService(repository);

        var dto = new CreateSaleDto(
            1,
            "CUS0000001",
            "EMP001",
            7000m,
            7000m,
            0m,
            []);

        var result = await service.CreateSaleAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Sale must include at least one item", result.ErrorMessage);
        Assert.Equal(0, repository.CreateSaleCallCount);
    }

    [Fact]
    public async Task CreateSaleAsync_WhenPayIsLessThanSubtotal_ReturnsFailureWithoutRepositoryCall()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesApplicationService(repository);

        var dto = new CreateSaleDto(
            1,
            "CUS0000001",
            "EMP001",
            7000m,
            6000m,
            0m,
            [
                new CreateSaleItemDto("P000000000001", 1, 7000m, 7000m)
            ]);

        var result = await service.CreateSaleAsync(dto);

        Assert.False(result.IsSuccess);
        Assert.Equal("Pay amount must be greater than or equal to subtotal", result.ErrorMessage);
        Assert.Equal(0, repository.CreateSaleCallCount);
    }

    [Fact]
    public async Task CreateSaleAsync_WhenRepositoryReturnsFalse_ReturnsFailure()
    {
        var repository = new FakeSalesRepository
        {
            NextCreateSaleResult = false
        };
        var service = new SalesApplicationService(repository);

        var result = await service.CreateSaleAsync(CreateValidRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed to create sale", result.ErrorMessage);
        Assert.Equal(1, repository.CreateSaleCallCount);
    }

    [Fact]
    public async Task CreateSaleAsync_WithValidRequest_MapsAndReturnsSuccess()
    {
        var repository = new FakeSalesRepository();
        var service = new SalesApplicationService(repository);

        var request = CreateValidRequest();
        var result = await service.CreateSaleAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(1, repository.CreateSaleCallCount);
        Assert.NotNull(repository.LastSale);
        Assert.NotNull(repository.LastDetails);
        Assert.Equal(request.CusId, repository.LastSale!.CustomerId);
        Assert.Equal(request.EmpId, repository.LastSale.EmployeeId);
        Assert.Single(repository.LastDetails!);
        Assert.Equal(request.Items[0].ProductId, repository.LastDetails[0].ProductId);
        Assert.Equal(request.Items[0].Qty, repository.LastDetails[0].Quantity);
    }

    private static CreateSaleDto CreateValidRequest()
        => new(
            1,
            "CUS0000001",
            "EMP001",
            7000m,
            7000m,
            0m,
            [
                new CreateSaleItemDto("P000000000001", 1, 7000m, 7000m)
            ]);

    private sealed class FakeSalesRepository : ISalesRepository
    {
        public int CreateSaleCallCount { get; private set; }
        public int GetSalesReportCallCount { get; private set; }
        public bool NextCreateSaleResult { get; set; } = true;
        public Sale? LastSale { get; private set; }
        public List<SaleDetail>? LastDetails { get; private set; }

        public Task<bool> CreateSaleAsync(Sale sale, IEnumerable<SaleDetail> details)
        {
            CreateSaleCallCount++;
            LastSale = sale;
            LastDetails = details.ToList();
            return Task.FromResult(NextCreateSaleResult);
        }

        public Task<List<SalesReportItem>> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            GetSalesReportCallCount++;
            return Task.FromResult(new List<SalesReportItem>());
        }
    }
}
