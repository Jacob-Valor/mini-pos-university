using System;
using System.Linq;
using System.Threading.Tasks;

using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class CustomerRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public CustomerRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddCustomerAsync_ThenSearchCustomersAsync_ReturnsInsertedCustomer()
    {
        var repo = new CustomerRepository(_fixture.ConnectionFactory);
        var customer = CreateCustomer();

        Assert.True(await repo.AddCustomerAsync(customer));

        var results = await repo.SearchCustomersAsync(customer.Name);
        var match = Assert.Single(results.Where(x => x.Id == customer.Id));
        Assert.Equal(customer.PhoneNumber, match.PhoneNumber);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WhenDataUnchanged_ReturnsTrue()
    {
        var repo = new CustomerRepository(_fixture.ConnectionFactory);
        var customer = CreateCustomer();
        Assert.True(await repo.AddCustomerAsync(customer));

        var success = await repo.UpdateCustomerAsync(customer);

        Assert.True(success);
    }

    [Fact]
    public async Task DeleteCustomerAsync_DeletesInsertedCustomer()
    {
        var repo = new CustomerRepository(_fixture.ConnectionFactory);
        var customer = CreateCustomer();
        Assert.True(await repo.AddCustomerAsync(customer));

        Assert.True(await repo.DeleteCustomerAsync(customer.Id));

        var customers = await repo.GetCustomersAsync();
        Assert.DoesNotContain(customers, x => x.Id == customer.Id);
    }

    private static Customer CreateCustomer()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new Customer
        {
            Id = $"C{suffix[..9]}",
            Name = $"Test{suffix[..5]}",
            Surname = "Customer",
            Gender = "F",
            Address = "Integration Street",
            PhoneNumber = $"020{suffix[..7]}"
        };
    }
}
