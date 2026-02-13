using System.Threading.Tasks;
using mini_pos.Services;
using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class EmployeeCredentialsRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public EmployeeCredentialsRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpdatePasswordAsync_ThenReadBack_ReturnsUpdatedHash()
    {
        var repo = new EmployeeCredentialsRepository(_fixture.ConnectionFactory);

        var newHash = "pbkdf2-sha256$1000$aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa$bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        Assert.True(await repo.UpdatePasswordAsync(_fixture.SeedEmployeeId, newHash));

        var stored = await repo.GetStoredPasswordHashAsync(_fixture.SeedEmployeeUsername);
        Assert.Equal(newHash, stored);
    }
}
