using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "ຍິນດີຕ້ອນຮັບສູ່ ໂປຣແກຣມຂາຍໜ້າຮ້ານ";

    [ObservableProperty]
    private string _currentUser = string.Empty;

    [ObservableProperty]
    private string _loginTime = string.Empty;

    [ObservableProperty]
    private string _currentDate = string.Empty;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public event EventHandler? LogoutRequested;

    private readonly IDatabaseService _databaseService;
    private readonly IDialogService _dialogService;
    private readonly IReportService _reportService;
    private readonly INavigationService _navigationService;
    private readonly Employee? _loggedInEmployee;

    public MainWindowViewModel(
        Employee? employee, 
        IDatabaseService databaseService, 
        IDialogService dialogService, 
        IReportService reportService,
        INavigationService navigationService)
    {
        _loggedInEmployee = employee;
        _databaseService = databaseService;
        _dialogService = dialogService;
        _reportService = reportService;
        _navigationService = navigationService;

        CurrentUser = employee != null 
            ? $"{employee.Name} {employee.Surname}" 
            : "Guest";
        LoginTime = DateTime.Now.ToString("HH:mm:ss");
        
        _ = UpdateDatePeriodicallyAsync();
    }

    [RelayCommand]
    private void Home()
    {
        CurrentPage = null;
    }

    [RelayCommand]
    private void ManageData()
    {
        Console.WriteLine("Manage Data clicked");
    }

    [RelayCommand]
    private void Import()
    {
        Console.WriteLine("Import clicked");
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
        Console.WriteLine("Reports clicked");
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
        Console.WriteLine("Settings clicked");
    }

    [RelayCommand]
    private void Logout()
    {
        Console.WriteLine("Logout clicked");
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

    private async Task UpdateDatePeriodicallyAsync()
    {
        while (true)
        {
            UpdateCurrentDate();
            await Task.Delay(1000);
        }
    }
}
