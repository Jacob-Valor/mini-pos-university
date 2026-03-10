using System;
using System.Linq;
using System.Threading.Tasks;

using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class EmployeeRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public EmployeeRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetEmployeeByUsernameAsync_ReturnsSeedEmployeeWithGeoData()
    {
        var repo = new EmployeeRepository(_fixture.ConnectionFactory);

        var employee = await repo.GetEmployeeByUsernameAsync(_fixture.SeedEmployeeUsername);

        Assert.NotNull(employee);
        Assert.Equal(_fixture.SeedEmployeeId, employee!.Id);
        Assert.Equal("Test Province", employee.Province);
        Assert.Equal("Test District", employee.District);
        Assert.Equal("Test Village", employee.Village);
    }

    [Fact]
    public async Task AddEmployeeAsync_ThenGetEmployeesAsync_ReturnsInsertedEmployee()
    {
        var repo = new EmployeeRepository(_fixture.ConnectionFactory);
        var employee = CreateEmployee();

        Assert.True(await repo.AddEmployeeAsync(employee));

        var employees = await repo.GetEmployeesAsync();
        var inserted = Assert.Single(employees.Where(x => x.Id == employee.Id));
        Assert.Equal(employee.Username, inserted.Username);
        Assert.Equal("010101", inserted.VillageId);
        Assert.Equal(DateTime.UtcNow.Date, inserted.StartDate.Date);
    }

    [Fact]
    public async Task UpdateEmployeeProfileAsync_UpdatesStoredFields()
    {
        var repo = new EmployeeRepository(_fixture.ConnectionFactory);
        var employee = CreateEmployee();
        Assert.True(await repo.AddEmployeeAsync(employee));

        employee.Name = "Updated";
        employee.Surname = "Profile";
        employee.PhoneNumber = "0209999999";
        employee.Username = $"upd{Guid.NewGuid():N}"[..12];

        Assert.True(await repo.UpdateEmployeeProfileAsync(employee));

        var updated = await repo.GetEmployeeByUsernameAsync(employee.Username);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated!.Name);
        Assert.Equal("Profile", updated.Surname);
        Assert.Equal("0209999999", updated.PhoneNumber);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_DeletesInsertedEmployee()
    {
        var repo = new EmployeeRepository(_fixture.ConnectionFactory);
        var employee = CreateEmployee();
        Assert.True(await repo.AddEmployeeAsync(employee));

        Assert.True(await repo.DeleteEmployeeAsync(employee.Id));

        var employees = await repo.GetEmployeesAsync();
        Assert.DoesNotContain(employees, x => x.Id == employee.Id);
    }

    private static Employee CreateEmployee()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new Employee
        {
            Id = $"E{suffix[..7]}",
            Name = "Integration",
            Surname = "Employee",
            Gender = "M",
            DateOfBirth = new DateTime(1995, 1, 1),
            VillageId = "010101",
            PhoneNumber = $"020{suffix[..7]}",
            Username = $"user{suffix[..8]}",
            Password = "hashed_password",
            Status = "Employee"
        };
    }
}
