using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

using mini_pos.Models;
using mini_pos.Services;
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
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<INavigationService, NavigationService>();

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
