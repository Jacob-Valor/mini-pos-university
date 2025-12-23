using ReactiveUI;
using System.Reactive;
using System;

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

    public string CurrentUser { get; } = "ສຸກສະຫວັນ ຈຸນດາລີ"; // Example user from image
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

    public MainWindowViewModel()
    {
        // Default to Home (or just empty for now, or keep the greeting logic separate?
        // For simplicity, let's make a simple HomeViewModel later or just use null to show the default greeting if we keep it,
        // but replacing the main content means we probably want a HomeViewModel.
        // For this task, I'll just initialize it to something empty or null and handle null in View?
        // Actually, to keep the Greeting, I might want to NOT replace everything, or move the Greeting to a HomeViewModel.
        // Let's create a placeholder for Home, but for now I'll just focus on Brand.
        // To avoid breaking the current "Greeting" display, we can make the View use a converter or triggers,
        // but simpler is to just have the Greeting be the default "Home" view logic.
        // I will make CurrentPage null by default, and if null, the View shows the Greeting.

        HomeCommand = ReactiveCommand.Create(() => { CurrentPage = null; });
        ManageDataCommand = ReactiveCommand.Create(() => Console.WriteLine("Manage Data clicked"));
        ImportCommand = ReactiveCommand.Create(() => Console.WriteLine("Import clicked"));
        CustomersCommand = ReactiveCommand.Create(() => Console.WriteLine("Customers clicked"));
        SaleCommand = ReactiveCommand.Create(() => Console.WriteLine("Sale clicked"));
        SearchCommand = ReactiveCommand.Create(() => Console.WriteLine("Search clicked"));
        ReportsCommand = ReactiveCommand.Create(() => Console.WriteLine("Reports clicked"));
        ProfileCommand = ReactiveCommand.Create(() => Console.WriteLine("Profile clicked"));
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
