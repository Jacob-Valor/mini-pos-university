using System.Linq;
using System.Threading.Tasks;

using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests.Integration;

[Collection("Integration")]
public sealed class GeoRepositoryIntegrationTests
{
    private readonly MariaDbFixture _fixture;

    public GeoRepositoryIntegrationTests(MariaDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProvincesAsync_ReturnsSeedProvince()
    {
        var repo = new GeoRepository(_fixture.ConnectionFactory);

        var provinces = await repo.GetProvincesAsync();

        Assert.Contains(provinces, x => x.Id == "01" && x.Name == "Test Province");
    }

    [Fact]
    public async Task GetDistrictsByProvinceAsync_ReturnsSeedDistrict()
    {
        var repo = new GeoRepository(_fixture.ConnectionFactory);

        var districts = await repo.GetDistrictsByProvinceAsync("01");

        var district = Assert.Single(districts.Where(x => x.Id == "0101"));
        Assert.Equal("Test District", district.Name);
    }

    [Fact]
    public async Task GetVillagesByDistrictAsync_ReturnsSeedVillage()
    {
        var repo = new GeoRepository(_fixture.ConnectionFactory);

        var villages = await repo.GetVillagesByDistrictAsync("0101");

        var village = Assert.Single(villages.Where(x => x.Id == "010101"));
        Assert.Equal("Test Village", village.Name);
    }
}
