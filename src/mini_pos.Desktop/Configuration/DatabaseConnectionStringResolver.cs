using System;
using MySqlConnector;

namespace mini_pos.Configuration;

public static class DatabaseConnectionStringResolver
{
    private const string PasswordEnvVarName = "DB_PASSWORD";

    public static string Resolve(string? configuredConnectionString, Func<string, string?>? envGetter = null)
    {
        if (string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not found. " +
                "Please configure ConnectionStrings:DefaultConnection (or ConnectionStrings__DefaultConnection)."
            );
        }

        MySqlConnectionStringBuilder builder;
        try
        {
            builder = new MySqlConnectionStringBuilder(configuredConnectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Database connection string format is invalid.", ex);
        }

        if (!string.IsNullOrWhiteSpace(builder.Password))
        {
            return builder.ConnectionString;
        }

        var readEnvironment = envGetter ?? Environment.GetEnvironmentVariable;
        var dbPassword = readEnvironment(PasswordEnvVarName);
        if (string.IsNullOrWhiteSpace(dbPassword))
        {
            throw new InvalidOperationException(
                $"Database password is missing. Set environment variable '{PasswordEnvVarName}' " +
                "or include Password in ConnectionStrings:DefaultConnection."
            );
        }

        builder.Password = dbPassword;
        return builder.ConnectionString;
    }
}
