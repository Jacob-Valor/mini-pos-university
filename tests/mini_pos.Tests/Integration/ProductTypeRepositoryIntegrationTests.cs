using System;
using System.Linq;
using System.Threading.Tasks;

using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class ProductTypeRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public ProductTypeRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddProductTypeAsync_ThenGetProductTypesAsync_ReturnsInsertedType()
    {
        var repo = new ProductTypeRepository(_fixture.ConnectionFactory);
        var type = CreateProductType();

        Assert.True(await repo.AddProductTypeAsync(type));

        var types = await repo.GetProductTypesAsync();
        var inserted = Assert.Single(types.Where(x => x.Id == type.Id));
        Assert.Equal(type.Name, inserted.Name);
    }

    [Fact]
    public async Task UpdateProductTypeAsync_WhenDataUnchanged_ReturnsTrue()
    {
        var repo = new ProductTypeRepository(_fixture.ConnectionFactory);
        var type = CreateProductType();
        Assert.True(await repo.AddProductTypeAsync(type));

        var success = await repo.UpdateProductTypeAsync(type);

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteProductTypeAsync_DeletesInsertedType()
    {
        var repo = new ProductTypeRepository(_fixture.ConnectionFactory);
        var type = CreateProductType();
        Assert.True(await repo.AddProductTypeAsync(type));

        Assert.True(await repo.DeleteProductTypeAsync(type.Id));

        var types = await repo.GetProductTypesAsync();
        Assert.DoesNotContain(types, x => x.Id == type.Id);
    }

    private static ProductType CreateProductType()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new ProductType
        {
            Id = $"T{suffix[..3]}",
            Name = $"Type {suffix[..6]}"
        };
    }
}
