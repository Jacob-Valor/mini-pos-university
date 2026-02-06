using System;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using MySqlConnector;
using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class SalesRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public SalesRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateSaleAsync_InsertsHeaderAndDetails_AndDecrementsStock()
    {
        var productRepo = new ProductRepository(_fixture.ConnectionFactory);
        var salesRepo = new SalesRepository(_fixture.ConnectionFactory);

        var barcode = ("T" + Guid.NewGuid().ToString("N"))[..13];

        var product = new Product
        {
            Barcode = barcode,
            ProductName = "Integration Product",
            Unit = "pcs",
            Quantity = 10,
            QuantityMin = 1,
            CostPrice = 5000,
            RetailPrice = 7000,
            BrandId = _fixture.SeedBrandId,
            CategoryId = _fixture.SeedCategoryId,
            Status = "ມີ"
        };

        Assert.True(await productRepo.AddProductAsync(product));

        var now = DateTime.UtcNow;
        var dateSale = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);

        var sale = new Sale
        {
            ExchangeRateId = _fixture.SeedExchangeRateId,
            CustomerId = _fixture.SeedCustomerId,
            EmployeeId = _fixture.SeedEmployeeId,
            DateSale = dateSale,
            SubTotal = 14000m,
            Pay = 14000m,
            Change = 0m
        };

        var details = new[]
        {
            new SaleDetail
            {
                ProductId = barcode,
                Quantity = 2,
                Price = 7000m,
                Total = 14000m
            }
        };

        Assert.True(await salesRepo.CreateSaleAsync(sale, details));

        var updated = await productRepo.GetProductByBarcodeAsync(barcode);
        Assert.NotNull(updated);
        Assert.Equal(8, updated!.Quantity);

        await using var connection = await _fixture.ConnectionFactory.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM sales_product WHERE product_id = @pid", connection);
        cmd.Parameters.Add(new MySqlParameter("@pid", MySqlDbType.VarChar) { Value = barcode });

        var rowCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        Assert.Equal(1, rowCount);
    }

    [Fact]
    public async Task CreateSaleAsync_WhenDetailProductMissing_RollsBackTransaction()
    {
        var salesRepo = new SalesRepository(_fixture.ConnectionFactory);

        var now = DateTime.UtcNow;
        var dateSale = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
        var subTotal = 12345.67m;

        var sale = new Sale
        {
            ExchangeRateId = _fixture.SeedExchangeRateId,
            CustomerId = _fixture.SeedCustomerId,
            EmployeeId = _fixture.SeedEmployeeId,
            DateSale = dateSale,
            SubTotal = subTotal,
            Pay = subTotal,
            Change = 0m
        };

        var details = new[]
        {
            new SaleDetail
            {
                ProductId = "DOES_NOT_EXIST",
                Quantity = 1,
                Price = subTotal,
                Total = subTotal
            }
        };

        Assert.False(await salesRepo.CreateSaleAsync(sale, details));

        await using var connection = await _fixture.ConnectionFactory.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM sales WHERE date_sale = @date AND subtotal = @sub", connection);
        cmd.Parameters.Add(new MySqlParameter("@date", MySqlDbType.DateTime) { Value = dateSale });
        cmd.Parameters.Add(new MySqlParameter("@sub", MySqlDbType.Decimal) { Precision = 12, Scale = 2, Value = subTotal });

        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        Assert.Equal(0, count);
    }
}
