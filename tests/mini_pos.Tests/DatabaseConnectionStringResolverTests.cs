using mini_pos.Configuration;
using MySqlConnector;
using Xunit;

namespace mini_pos.Tests;

public sealed class DatabaseConnectionStringResolverTests
{
    [Fact]
    public void Resolve_WhenPasswordProvidedInConnectionString_PreservesConfiguredPassword()
    {
        var resolved = DatabaseConnectionStringResolver.Resolve(
            "Server=localhost;Port=3306;Database=mini_pos;User=root;Password=config_pwd;"
        );

        var builder = new MySqlConnectionStringBuilder(resolved);
        Assert.Equal("config_pwd", builder.Password);
    }

    [Fact]
    public void Resolve_WhenPasswordMissing_UsesEnvironmentPassword()
    {
        var resolved = DatabaseConnectionStringResolver.Resolve(
            "Server=localhost;Port=3306;Database=mini_pos;User=root;",
            key => key == "DB_PASSWORD" ? "env_pwd" : null
        );

        var builder = new MySqlConnectionStringBuilder(resolved);
        Assert.Equal("env_pwd", builder.Password);
    }

    [Fact]
    public void Resolve_WhenPasswordMissingAndEnvironmentPasswordNotSet_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConnectionStringResolver.Resolve(
                "Server=localhost;Port=3306;Database=mini_pos;User=root;",
                _ => null
            )
        );

        Assert.Contains("DB_PASSWORD", exception.Message);
    }
}
