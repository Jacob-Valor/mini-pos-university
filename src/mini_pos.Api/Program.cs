using System;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using mini_pos.Api.Services;
using mini_pos.Configuration;
using mini_pos.Services;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var baseDir = AppContext.BaseDirectory;

if (!File.Exists(Path.Combine(baseDir, "appsettings.json")))
{
    var currentDir = Directory.GetCurrentDirectory();
    if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        baseDir = currentDir;
}

DotEnvLoader.LoadFromSearchPaths(baseDir, Directory.GetCurrentDirectory(), AppContext.BaseDirectory);

builder.Configuration
    .SetBasePath(baseDir)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.DefaultConnection), "ConnectionStrings:DefaultConnection is required")
    .ValidateOnStart();

builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddSingleton<IBrandRepository, BrandRepository>();
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IProductTypeRepository, ProductTypeRepository>();
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<ISupplierRepository, SupplierRepository>();
builder.Services.AddSingleton<IExchangeRateRepository, ExchangeRateRepository>();
builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddSingleton<ISalesRepository, SalesRepository>();
builder.Services.AddScoped<ISalesApplicationService, SalesApplicationService>();

var app = builder.Build();

_ = app.Services.GetRequiredService<IMySqlConnectionFactory>();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapControllers();

var port = builder.Configuration["PORT"] ?? "5000";
Log.Information("Starting mini_pos API on port {Port}", port);

app.Run($"http://0.0.0.0:{port}");
