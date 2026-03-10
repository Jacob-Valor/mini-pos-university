using System;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mini_pos.Models;
using mini_pos.Services;

using Serilog;

namespace mini_pos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "ຍິນດີຕ້ອນຮັບສູ່ ໂປຣແກຣມຂາຍໜ້າຮ້ານ";

    [ObservableProperty]
    private string _currentUser = string.Empty;

    [ObservableProperty]
    private string _currentRole = string.Empty;

    [ObservableProperty]
    private string _currentRoleDisplay = string.Empty;

    [ObservableProperty]
    private string _loginTime = string.Empty;

    [ObservableProperty]
    private string _currentDate = string.Empty;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private StreamGeometry _themeIcon;

    [ObservableProperty]
    private string _themeText;

    public event EventHandler? LogoutRequested;

    private readonly CancellationTokenSource _dateUpdateCts = new();

    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;
    private readonly Employee? _loggedInEmployee;

    public MainWindowViewModel(
        Employee? employee,
        IDialogService dialogService,
        INavigationService navigationService,
        IThemeService themeService)
    {
        _loggedInEmployee = employee;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _themeService = themeService;

        _themeIcon = GetThemeIcon();
        _themeText = GetThemeText();

        CurrentUser = employee != null
            ? $"{employee.Name} {employee.Surname}"
            : "Guest";

        CurrentRole = employee?.Position ?? string.Empty;
        CurrentRoleDisplay = TranslateRoleToLao(CurrentRole);
        LoginTime = DateTime.Now.ToString("HH:mm:ss");

        UpdateCurrentDate();
        _ = UpdateDatePeriodicallyAsync(_dateUpdateCts.Token);
    }

    private StreamGeometry GetThemeIcon()
    {
        var app = Application.Current;
        var iconKey = _themeService.IsDarkTheme ? "IconSun" : "IconMoon";
        if (app != null && app.TryGetResource(iconKey, app.ActualThemeVariant, out object? resource) && resource is StreamGeometry geometry)
        {
            return geometry;
        }
        return new StreamGeometry();
    }

    private string GetThemeText()
    {
        return _themeService.IsDarkTheme ? "ໂໝດແສງ" : "ໂໝດມື່ອ";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        ThemeIcon = GetThemeIcon();
        ThemeText = GetThemeText();
    }

    private static string TranslateRoleToLao(string role)
    {
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            return "ແອັດມິນ";

        if (string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase))
            return "ພະນັກງານ";

        return role;
    }

    [RelayCommand]
    private void Home()
    {
        CurrentPage = null;
    }

    [RelayCommand]
    private void ManageData()
    {
        Log.Debug("Manage Data clicked");
    }

    [RelayCommand]
    private void Import()
    {
        Log.Debug("Import clicked");
    }

    [RelayCommand]
    private void Customers()
    {
        CurrentPage = _navigationService.CreateViewModel<CustomerViewModel>();
    }

    [RelayCommand]
    private void Sale()
    {
        if (_loggedInEmployee != null)
        {
            CurrentPage = _navigationService.CreateViewModelWithArgs<SalesViewModel>(_loggedInEmployee);
        }
        else
        {
            _ = _dialogService.ShowErrorAsync("ບໍ່ສາມາດຂາຍໄດ້: ບໍ່ມີຜູ້ໃຊ້ເຂົ້າສູ່ລະບົບ");
        }
    }

    [RelayCommand]
    private void Search()
    {
        CurrentPage = _navigationService.CreateViewModel<ProductViewModel>();
    }

    [RelayCommand]
    private void Reports()
    {
        Log.Debug("Reports clicked");
    }

    [RelayCommand]
    private void Profile()
    {
        if (_loggedInEmployee != null)
        {
            CurrentPage = _navigationService.CreateViewModelWithArgs<ProfileViewModel>(_loggedInEmployee);
        }
        else
        {
            _ = _dialogService.ShowErrorAsync("ບໍ່ມີຂໍ້ມູນຜູ້ໃຊ້");
        }
    }

    [RelayCommand]
    private void Settings()
    {
        Log.Debug("Settings clicked");
    }

    [RelayCommand]
    private void Logout()
    {
        Log.Information("Logout requested");
        _dateUpdateCts.Cancel();
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void GoToBrand()
    {
        CurrentPage = _navigationService.CreateViewModel<BrandViewModel>();
    }

    [RelayCommand]
    private void GoToProductType()
    {
        CurrentPage = _navigationService.CreateViewModel<ProductTypeViewModel>();
    }

    [RelayCommand]
    private void GoToProduct()
    {
        CurrentPage = _navigationService.CreateViewModel<ProductViewModel>();
    }

    [RelayCommand]
    private void GoToEmployee()
    {
        CurrentPage = _navigationService.CreateViewModel<EmployeeViewModel>();
    }

    [RelayCommand]
    private void GoToExchangeRate()
    {
        CurrentPage = _navigationService.CreateViewModel<ExchangeRateViewModel>();
    }

    [RelayCommand]
    private void GoToSupplier()
    {
        CurrentPage = _navigationService.CreateViewModel<SupplierViewModel>();
    }

    [RelayCommand]
    private void GoToSalesReport()
    {
        CurrentPage = _navigationService.CreateViewModel<SalesReportViewModel>();
    }

    private void UpdateCurrentDate()
    {
        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private async Task UpdateDatePeriodicallyAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateCurrentDate();
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation on logout/close.
        }
    }
}
