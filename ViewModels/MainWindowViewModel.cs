using ReactiveUI;
using System.Reactive;
using System;
using mini_pos.Models;

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

    public string CurrentDate { get; } = DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm:ss tt");

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

    public MainWindowViewModel(Employee? employee = null)
    {
        // Set the current user from the logged-in employee
        if (employee != null)
        {
            _loggedInEmployee = employee;
            CurrentUser = $"{employee.Name} {employee.Surname}";
        }

        HomeCommand = ReactiveCommand.Create(() => { CurrentPage = null; });
        ManageDataCommand = ReactiveCommand.Create(() => Console.WriteLine("Manage Data clicked"));
        ImportCommand = ReactiveCommand.Create(() => Console.WriteLine("Import clicked"));
        CustomersCommand = ReactiveCommand.Create(() => { CurrentPage = new CustomerViewModel(); });
        SaleCommand = ReactiveCommand.Create(() => { CurrentPage = new SalesViewModel(); });
        SearchCommand = ReactiveCommand.Create(() => Console.WriteLine("Search clicked"));
        ReportsCommand = ReactiveCommand.Create(() => Console.WriteLine("Reports clicked"));
        
        ProfileCommand = ReactiveCommand.Create(() => 
        { 
            if (_loggedInEmployee != null)
            {
                CurrentPage = new ProfileViewModel(_loggedInEmployee); 
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

        GoToBrandCommand = ReactiveCommand.Create(() => { CurrentPage = new BrandViewModel(); });
        GoToProductTypeCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductTypeViewModel(); });
        GoToProductCommand = ReactiveCommand.Create(() => { CurrentPage = new ProductViewModel(); });
        GoToEmployeeCommand = ReactiveCommand.Create(() => { CurrentPage = new EmployeeViewModel(); });
        GoToExchangeRateCommand = ReactiveCommand.Create(() => { CurrentPage = new ExchangeRateViewModel(); });
        GoToSupplierCommand = ReactiveCommand.Create(() => { CurrentPage = new SupplierViewModel(); });
    }
}
