using ReactiveUI;
using System.Reactive;
using System;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public string Greeting { get; } = "ຍິນດີຕ້ອນຮັບສູ່ ໂປຣແກຣມຂາຍໜ້າຮ້ານ";

    public ReactiveCommand<Unit, Unit> HomeCommand { get; }
    public ReactiveCommand<Unit, Unit> ManageDataCommand { get; }
    public ReactiveCommand<Unit, Unit> ImportCommand { get; }
    public ReactiveCommand<Unit, Unit> CustomersCommand { get; }
    public ReactiveCommand<Unit, Unit> SaleCommand { get; }
    public ReactiveCommand<Unit, Unit> SearchCommand { get; }
    public ReactiveCommand<Unit, Unit> ReportsCommand { get; }
    public ReactiveCommand<Unit, Unit> ProfileCommand { get; }
    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

    public event EventHandler? LogoutRequested;

    private Employee? _loggedInEmployee;

    private string _currentUser = string.Empty;
    public string CurrentUser
    {
        get => _currentUser;
        set => this.RaiseAndSetIfChanged(ref _currentUser, value);
    }

    private string _loginTime = string.Empty;
    public string LoginTime
    {
        get => _loginTime;
        set => this.RaiseAndSetIfChanged(ref _loginTime, value);
    }

    private string _currentDate = string.Empty;
    public string CurrentDate
    {
        get => _currentDate;
        private set => this.RaiseAndSetIfChanged(ref _currentDate, value);
    }

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }
    private ViewModelBase? _currentPage;

    public ReactiveCommand<Unit, Unit> GoToBrandCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToProductTypeCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToProductCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToExchangeRateCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToSupplierCommand { get; }

    private readonly IDatabaseService _databaseService;
    private readonly IDialogService _dialogService;

    public MainWindowViewModel(Employee? employee, IDatabaseService databaseService, IDialogService dialogService)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        _loggedInEmployee = employee;
        if (_loggedInEmployee != null)
        {
            CurrentUser = $"{_loggedInEmployee.Name} {_loggedInEmployee.Surname}";
        }
        else
        {
            CurrentUser = "Guest";
        }

        LoginTime = DateTime.Now.ToString("HH:mm:ss");

        _ = UpdateDatePeriodicallyAsync();

        CustomersCommand = ReactiveCommand.Create(() => { CurrentPage = new CustomerViewModel(_databaseService, _dialogService); });
        SaleCommand = ReactiveCommand.Create(() =>
        {
            if (_loggedInEmployee != null)
            {
                CurrentPage = new SalesViewModel(_loggedInEmployee, _databaseService, _dialogService);
            }
            else
            {
                _ = _dialogService.ShowErrorAsync("ບໍ່ສາມາດຂາຍໄດ້: ບໍ່ມີຜູ້ໃຊ້ເຂົ້າສູ່ລະບົບ");
            }
        });
        SearchCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = new ProductViewModel(_databaseService, _dialogService);
        });

        ProfileCommand = ReactiveCommand.Create(() =>
        {
            if (_loggedInEmployee != null)
            {
                CurrentPage = new ProfileViewModel(_loggedInEmployee, _databaseService, _dialogService);
            }
            else
            {
                _ = _dialogService.ShowErrorAsync("ບໍ່ມີຂໍ້ມູນຜູ້ໃຊ້");
            }
        });

        SettingsCommand = ReactiveCommand.Create(() => Console.WriteLine("Settings clicked"));
        LogoutCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Logout clicked");
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        });

        GoToBrandCommand = ReactiveCommand.Create(() => { CurrentPage = new BrandViewModel(_databaseService, _dialogService); });
        GoToProductTypeCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductTypeViewModel(_databaseService, _dialogService); });
        GoToProductCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductViewModel(_databaseService, _dialogService); });
        GoToEmployeeCommand = ReactiveCommand.Create(() => { CurrentPage = new EmployeeViewModel(_databaseService, _dialogService); });
        GoToExchangeRateCommand = ReactiveCommand.Create(() => { CurrentPage = new ExchangeRateViewModel(_databaseService, _dialogService); });
        GoToSupplierCommand = ReactiveCommand.Create(() => { CurrentPage = new SupplierViewModel(_databaseService, _dialogService); });

        HomeCommand = ReactiveCommand.Create(() =>
        {
            CurrentPage = null;
        });

        ManageDataCommand = ReactiveCommand.Create(() => Console.WriteLine("Manage Data clicked"));
        ImportCommand = ReactiveCommand.Create(() => Console.WriteLine("Import clicked"));
        ReportsCommand = ReactiveCommand.Create(() => Console.WriteLine("Reports clicked"));
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
