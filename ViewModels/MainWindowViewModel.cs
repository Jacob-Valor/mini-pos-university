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

    // Commands for specific pages
    public ReactiveCommand<Unit, Unit> GoToBrandCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToProductTypeCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToProductCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToEmployeeCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToExchangeRateCommand { get; }
    public ReactiveCommand<Unit, Unit> GoToSupplierCommand { get; }

    private readonly IDatabaseService _databaseService;

    public MainWindowViewModel(Employee? employee, IDatabaseService databaseService)
    {
        _databaseService = databaseService;
        // Set the current user from the logged-in employee
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

        // Start clock
        _ = UpdateDatePeriodicallyAsync();

        // Default Page (Dashboard or Empty)
        // Pass Dependencies to Child ViewModels
        CustomersCommand = ReactiveCommand.Create(() => { CurrentPage = new CustomerViewModel(_databaseService); });
        SaleCommand = ReactiveCommand.Create(() => 
        { 
            if (_loggedInEmployee != null)
            {
                CurrentPage = new SalesViewModel(_loggedInEmployee, _databaseService); 
            }
            else
            {
                Console.WriteLine("Cannot open Sales: No logged in user.");
            }
        });
        SearchCommand = ReactiveCommand.Create(() => 
        { 
             CurrentPage = new ProductViewModel(_databaseService);
        });
        
        ProfileCommand = ReactiveCommand.Create(() => 

        { 
            if (_loggedInEmployee != null)
            {
                CurrentPage = new ProfileViewModel(_loggedInEmployee, _databaseService); 
            }
            else
            {
                Console.WriteLine("No logged in employee to show profile for.");
            }
        });

        SettingsCommand = ReactiveCommand.Create(() => Console.WriteLine("Settings clicked"));
        LogoutCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Logout clicked");
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        });

        GoToBrandCommand = ReactiveCommand.Create(() => { CurrentPage = new BrandViewModel(_databaseService); });
        GoToProductTypeCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductTypeViewModel(_databaseService); });
        GoToProductCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductViewModel(_databaseService); });
        GoToEmployeeCommand = ReactiveCommand.Create(() => { CurrentPage = new EmployeeViewModel(_databaseService); });
        GoToExchangeRateCommand = ReactiveCommand.Create(() => { CurrentPage = new ExchangeRateViewModel(_databaseService); });
        GoToSupplierCommand = ReactiveCommand.Create(() => { CurrentPage = new SupplierViewModel(_databaseService); });

        // Initialize commands
        HomeCommand = ReactiveCommand.Create(() => 
        { 
            // Reset CurrentPage to null to show the welcome screen (Greeting)
            CurrentPage = null; 
        });
        
        ManageDataCommand = ReactiveCommand.Create(() => Console.WriteLine("Manage Data clicked"));
        ImportCommand = ReactiveCommand.Create(() => Console.WriteLine("Import clicked"));
        ReportsCommand = ReactiveCommand.Create(() => Console.WriteLine("Reports clicked"));
    }

    /// <summary>
    /// Updates the current date/time display.
    /// </summary>
    private void UpdateCurrentDate()
    {
        // Lao format example: ວັນຈັນ, 12 ມັງກອນ 2026 10:30:00 AM
        // Using standard format for now
        CurrentDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    /// <summary>
    /// Updates date/time display every second.
    /// </summary>
    private async Task UpdateDatePeriodicallyAsync()
    {
        while (true)
        {
            UpdateCurrentDate();
            await Task.Delay(1000);
        }
    }
}
