using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class ExchangeRateRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public ExchangeRateRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddExchangeRateAsync_ThenGetLatest_ReturnsInsertedRate()
    {
        var repo = new ExchangeRateRepository(_fixture.ConnectionFactory);

        var inserted = new ExchangeRate
        {
            UsdRate = 23456.78m,
            ThbRate = 789.12m,
            CreatedDate = System.DateTime.UtcNow
        };

        Assert.True(await repo.AddExchangeRateAsync(inserted));

        var latest = await repo.GetLatestExchangeRateAsync();
        Assert.NotNull(latest);
        Assert.Equal(inserted.UsdRate, latest!.UsdRate);
        Assert.Equal(inserted.ThbRate, latest.ThbRate);
    }
}
