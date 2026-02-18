using System;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class ProductRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public ProductRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdateProductAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        var repo = new ProductRepository(_fixture.ConnectionFactory);
        var missingBarcode = ("M" + Guid.NewGuid().ToString("N"))[..13];

        var product = CreateProduct(missingBarcode);

        var success = await repo.UpdateProductAsync(product);

        Assert.False(success);
    }

    [Fact]
    public async Task UpdateProductAsync_WhenDataUnchanged_ReturnsTrue()
    {
        var repo = new ProductRepository(_fixture.ConnectionFactory);
        var barcode = ("U" + Guid.NewGuid().ToString("N"))[..13];

        var product = CreateProduct(barcode);
        Assert.True(await repo.AddProductAsync(product));

        var success = await repo.UpdateProductAsync(product);

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteProductAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        var repo = new ProductRepository(_fixture.ConnectionFactory);
        var missingBarcode = ("D" + Guid.NewGuid().ToString("N"))[..13];

        var success = await repo.DeleteProductAsync(missingBarcode);

        Assert.False(success);
    }

    private Product CreateProduct(string barcode)
    {
        return new Product
        {
            Barcode = barcode,
            ProductName = "Repository Test Product",
            Unit = "pcs",
            Quantity = 10,
            QuantityMin = 1,
            CostPrice = 1000m,
            RetailPrice = 1200m,
            BrandId = _fixture.SeedBrandId,
            CategoryId = _fixture.SeedCategoryId,
            Status = "ມີ"
        };
    }
}
