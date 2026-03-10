using System;
using System.Linq;
using System.Threading.Tasks;

using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class SupplierRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public SupplierRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddSupplierAsync_ThenGetSuppliersAsync_ReturnsInsertedSupplier()
    {
        var repo = new SupplierRepository(_fixture.ConnectionFactory);
        var supplier = CreateSupplier();

        Assert.True(await repo.AddSupplierAsync(supplier));

        var suppliers = await repo.GetSuppliersAsync();
        var inserted = Assert.Single(suppliers.Where(x => x.Id == supplier.Id));
        Assert.Equal(supplier.ContactName, inserted.ContactName);
    }

    [Fact]
    public async Task UpdateSupplierAsync_WhenDataUnchanged_ReturnsTrue()
    {
        var repo = new SupplierRepository(_fixture.ConnectionFactory);
        var supplier = CreateSupplier();
        Assert.True(await repo.AddSupplierAsync(supplier));

        var success = await repo.UpdateSupplierAsync(supplier);

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteSupplierAsync_DeletesInsertedSupplier()
    {
        var repo = new SupplierRepository(_fixture.ConnectionFactory);
        var supplier = CreateSupplier();
        Assert.True(await repo.AddSupplierAsync(supplier));

        Assert.True(await repo.DeleteSupplierAsync(supplier.Id));

        var suppliers = await repo.GetSuppliersAsync();
        Assert.DoesNotContain(suppliers, x => x.Id == supplier.Id);
    }

    private static Supplier CreateSupplier()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new Supplier
        {
            Id = $"S{suffix[..9]}",
            Name = $"Supplier {suffix[..5]}",
            ContactName = "Contact Person",
            Email = $"{suffix[..6]}@example.com",
            Phone = $"020{suffix[..7]}",
            Address = "Supplier Street"
        };
    }
}
