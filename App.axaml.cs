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
        // Configure Dependency Injection
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
        // Register Services
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IDialogService, DialogService>();

        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>(); 
        services.AddTransient<ProductViewModel>();
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        // Resolve LoginViewModel from DI container
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
        // Use ActivatorUtilities to create MainWindowViewModel with mixed dependencies
        // (IDatabaseService from DI, Employee passed manually)
        // Note: We check if employee is null, although generally it shouldn't be here.
        // If it is, ActivatorUtilities might warn but it will pass null to constructor.
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
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
