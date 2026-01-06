using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using mini_pos.Models;
using mini_pos.ViewModels;
using mini_pos.Views;

namespace mini_pos;

public partial class App : Application
{
    public override void Initialize()
    {
        DbusSafeSynchronizationContext.InstallIfNeeded();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            ShowLogin(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginViewModel = new LoginViewModel();
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
        var mainWindowViewModel = new MainWindowViewModel(employee);
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
