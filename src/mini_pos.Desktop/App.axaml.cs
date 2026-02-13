using System;
using System.Linq;
using System.IO;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using mini_pos.Models;
using mini_pos.Configuration;
using mini_pos.Services;
using mini_pos.Validators;
using mini_pos.ViewModels;
using mini_pos.Views;

namespace mini_pos;

public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        ConfigureServices(collection);
        ServiceProvider = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            ShowLogin(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuration = BuildConfiguration();
        services.AddSingleton<IConfiguration>(configuration);

        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.DefaultConnection), "ConnectionStrings:DefaultConnection is required")
            .ValidateOnStart();

        services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();
        services.AddSingleton<IBrandRepository, BrandRepository>();
        services.AddSingleton<ICustomerRepository, CustomerRepository>();
        services.AddSingleton<IProductTypeRepository, ProductTypeRepository>();
        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddSingleton<ISupplierRepository, SupplierRepository>();
        services.AddSingleton<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddSingleton<IGeoRepository, GeoRepository>();
        services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
        services.AddSingleton<IEmployeeCredentialsRepository, EmployeeCredentialsRepository>();
        services.AddSingleton<ISalesRepository, SalesRepository>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IValidator<Product>, ProductValidator>();
        services.AddSingleton<IValidator<Customer>, CustomerValidator>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<BrandViewModel>();
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<EmployeeViewModel>();
        services.AddTransient<ExchangeRateViewModel>();
        services.AddTransient<ProductTypeViewModel>();
        services.AddTransient<ProductViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<ReceiptViewModel>();
        services.AddTransient<SalesReportViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<SupplierViewModel>();
    }

    private static IConfiguration BuildConfiguration()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var baseDir = AppContext.BaseDirectory;

        if (!File.Exists(Path.Combine(baseDir, "appsettings.json")))
        {
            var currentDir = Directory.GetCurrentDirectory();
            if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
                baseDir = currentDir;
        }

        return new ConfigurationBuilder()
            .SetBasePath(baseDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginViewModel = ServiceProvider!.GetRequiredService<LoginViewModel>();

        var loginView = new LoginView
        {
            DataContext = loginViewModel,
        };

        loginViewModel.LoginSuccessful += (s, e) =>
        {
            ShowMainWindow(desktop, loginViewModel.CurrentEmployee);
            loginView.Close();
        };

        desktop.MainWindow = loginView;
        loginView.Show();
    }

    private void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop, Employee? employee)
    {
        var mainWindowViewModel = ActivatorUtilities.CreateInstance<MainWindowViewModel>(
            ServiceProvider!,
            employee!
        );

        var mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel,
        };

        mainWindowViewModel.LogoutRequested += (s, e) =>
        {
            ShowLogin(desktop);
            mainWindow.Close();
        };

        desktop.MainWindow = mainWindow;
        mainWindow.Show();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
