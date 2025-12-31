using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

/// <summary>
/// ViewModel for managing customer data (ຈັດການຂໍ້ມູນລູກຄ້າ)
/// </summary>
public class CustomerViewModel : ViewModelBase
{
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

    public CustomerViewModel()
    {
        // Initialize commands with proper canExecute observables
        var canAdd = this.WhenAnyValue(x => x.Name)
                         .Select(name => !string.IsNullOrWhiteSpace(name));

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedCustomer)
                                  .Select(customer => customer != null);

        AddCommand = ReactiveCommand.Create(Add, canAdd);
        EditCommand = ReactiveCommand.Create(Edit, canEditOrDelete);
        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(ClearForm);

        // Load mock data
        LoadMockData();
        GenerateNewId();
    }

    #endregion

    #region Private Methods

    private void LoadMockData()
    {
        AllCustomers.Add(new Customer 
        { 
            Id = "CUS0000001", 
            Name = "ສົມໃຈ", 
            Surname = "ໃຈດີ", 
            Gender = "ຊາຍ", 
            PhoneNumber = "02055555555", 
            Address = "ວຽງຈັນ" 
        });
        
        AllCustomers.Add(new Customer 
        { 
            Id = "CUS0000002", 
            Name = "ມະນີ", 
            Surname = "ແກ້ວ", 
            Gender = "ຍິງ", 
            PhoneNumber = "02099999999", 
            Address = "ປາກເຊ" 
        });

        FilterCustomers();
    }

    private void GenerateNewId()
    {
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

    private void Add()
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

        AllCustomers.Add(newCustomer);
        FilterCustomers();
        ClearForm();
    }

    private void Edit()
    {
        if (SelectedCustomer == null) return;

        // Find and update the customer in the master list
        var index = AllCustomers.IndexOf(SelectedCustomer);
        if (index >= 0)
        {
            AllCustomers[index] = new Customer
            {
                Id = SelectedCustomer.Id,
                Name = Name,
                Surname = Surname,
                Gender = Gender,
                PhoneNumber = PhoneNumber,
                Address = Address
            };
        }

        FilterCustomers();
        ClearForm();
    }

    private void Delete()
    {
        if (SelectedCustomer == null) return;

        AllCustomers.Remove(SelectedCustomer);
        FilterCustomers();
        ClearForm();
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
