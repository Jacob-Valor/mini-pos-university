using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace mini_pos.Services;

/// <summary>
/// Provides configuration management for the Mini POS application.
/// Reads settings from appsettings.json and environment variables.
/// </summary>
public class ConfigurationService
{
    private static ConfigurationService? _instance;
    private static readonly object _lock = new();
    
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Gets the singleton instance of ConfigurationService.
    /// </summary>
    public static ConfigurationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ConfigurationService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Private constructor - use Instance property to access.
    /// </summary>
    private ConfigurationService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(GetBasePath())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    /// <summary>
    /// Reinitializes the singleton instance (useful for testing).
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Gets the base path for configuration files.
    /// Handles both development and published scenarios.
    /// </summary>
    private static string GetBasePath()
    {
        // Try AppContext.BaseDirectory first (works for published apps)
        var baseDir = AppContext.BaseDirectory;
        
        // Check if appsettings.json exists in base directory
        if (File.Exists(Path.Combine(baseDir, "appsettings.json")))
        {
            return baseDir;
        }

        // Fall back to current directory (development scenario)
        var currentDir = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return currentDir;
        }

        // Default to base directory
        return baseDir;
    }

    /// <summary>
    /// Gets the database connection string.
    /// Priority: Environment variable > appsettings.json
    /// </summary>
    /// <returns>The database connection string.</returns>
    public string GetConnectionString()
    {
        // First check environment variable (for Docker deployment)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // Fall back to appsettings.json
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string not found. " +
                "Please check appsettings.json or set ConnectionStrings__DefaultConnection environment variable.");
        }

        return connectionString;
    }

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value or null.</returns>
    public string? GetValue(string key)
    {
        return _configuration[key];
    }

    /// <summary>
    /// Gets a configuration section.
    /// </summary>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The configuration section.</returns>
    public IConfigurationSection GetSection(string sectionName)
    {
        return _configuration.GetSection(sectionName);
    }
}
