using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using mini_pos.Models;
using mini_pos.Services;
using ReactiveUI;

namespace mini_pos.ViewModels;

/// <summary>
/// ViewModel for managing customer data (ຈັດການຂໍ້ມູນລູກຄ້າ)
/// </summary>
public class CustomerViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;

    #region Private Fields

    private string _customerId = string.Empty;
    private string _name = string.Empty;
    private string _surname = string.Empty;
    private string _gender = "ຊາຍ";
    private string _phoneNumber = string.Empty;
    private string _address = string.Empty;
    private string _searchText = string.Empty;
    private Customer? _selectedCustomer;

    #endregion

    #region Collections

    /// <summary>
    /// Master list of all customers (used for filtering)
    /// </summary>
    public ObservableCollection<Customer> AllCustomers { get; } = new();

    /// <summary>
    /// Filtered list of customers displayed in the UI
    /// </summary>
    public ObservableCollection<Customer> Customers { get; } = new();

    #endregion

    #region Properties

    public string CustomerId
    {
        get => _customerId;
        set => this.RaiseAndSetIfChanged(ref _customerId, value);
    }

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Surname
    {
        get => _surname;
        set => this.RaiseAndSetIfChanged(ref _surname, value);
    }

    public string Gender
    {
        get => _gender;
        set => this.RaiseAndSetIfChanged(ref _gender, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    public string Address
    {
        get => _address;
        set => this.RaiseAndSetIfChanged(ref _address, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterCustomers();
        }
    }

    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCustomer, value);
            if (value != null)
            {
                PopulateFormFromCustomer(value);
            }
        }
    }

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion

    #region Constructor

    public CustomerViewModel(IDatabaseService databaseService)
    {
        _databaseService = databaseService;

        // Initialize commands with proper canExecute observables
        var canAdd = this.WhenAnyValue(x => x.Name)
                         .Select(name => !string.IsNullOrWhiteSpace(name));

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedCustomer)
                                  .Select(customer => customer != null);

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync, canAdd);
        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(ClearForm);

        // Load data
        _ = LoadDataAsync();
    }

    public CustomerViewModel() : this(null!)
    {
        // Design-time
    }

    #endregion

    #region Private Methods

    private async Task LoadDataAsync()
    {
        if (_databaseService == null) return;
        
        AllCustomers.Clear();
        var customers = await _databaseService.GetCustomersAsync();
        foreach (var c in customers)
        {
            AllCustomers.Add(c);
        }
        FilterCustomers();
        GenerateNewId();
    }

    private void GenerateNewId()
    {
        // In a real app, we might query MAX(id) from DB or use AutoIncrement
        // For now, simple counting logic based on loaded list
        int count = AllCustomers.Count + 1;
        CustomerId = $"CUS{count:D7}";
    }

    private void PopulateFormFromCustomer(Customer customer)
    {
        CustomerId = customer.Id;
        Name = customer.Name;
        Surname = customer.Surname;
        Gender = customer.Gender;
        PhoneNumber = customer.PhoneNumber;
        Address = customer.Address;
    }

    private void FilterCustomers()
    {
        Customers.Clear();
        var query = AllCustomers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(c => 
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Surname.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var customer in query)
        {
            Customers.Add(customer);
        }
    }

    private async Task AddAsync()
    {
        var newCustomer = new Customer
        {
            Id = CustomerId,
            Name = Name,
            Surname = Surname,
            Gender = Gender,
            PhoneNumber = PhoneNumber,
            Address = Address
        };

        bool success = await _databaseService.AddCustomerAsync(newCustomer);
        if (success)
        {
            await LoadDataAsync();
            ClearForm();
        }
    }

    private async Task EditAsync()
    {
        if (SelectedCustomer == null) return;

        var updateCustomer = new Customer
        {
            Id = SelectedCustomer.Id,
            Name = Name,
            Surname = Surname,
            Gender = Gender,
            PhoneNumber = PhoneNumber,
            Address = Address
        };

        bool success = await _databaseService.UpdateCustomerAsync(updateCustomer);
        if (success)
        {
            await LoadDataAsync();
            ClearForm();
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedCustomer == null) return;

        bool success = await _databaseService.DeleteCustomerAsync(SelectedCustomer.Id);
        if (success)
        {
            await LoadDataAsync();
            ClearForm();
        }
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        Name = string.Empty;
        Surname = string.Empty;
        Gender = "ຊາຍ";
        PhoneNumber = string.Empty;
        Address = string.Empty;
        GenerateNewId();
    }

    #endregion
}
