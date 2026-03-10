using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mini_pos.Models;
using mini_pos.Services;
using mini_pos.Validators;

namespace mini_pos.ViewModels;

/// <summary>
/// ViewModel for managing customer data (ຈັດການຂໍ້ມູນລູກຄ້າ)
/// </summary>
public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDialogService? _dialogService;
    private readonly IValidator<Customer> _customerValidator;

    [ObservableProperty]
    private string _customerId = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    partial void OnNameChanged(string value) => CanAdd = !string.IsNullOrWhiteSpace(value);

    [ObservableProperty]
    private string _surname = string.Empty;

    [ObservableProperty]
    private string _gender = "ຊາຍ";

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterCustomers();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value != null) PopulateFormFromCustomer(value);
        CanEditOrDelete = value != null;
    }

    [ObservableProperty]
    private bool _canAdd;

    [ObservableProperty]
    private bool _canEditOrDelete;

    public ObservableCollection<Customer> AllCustomers { get; } = new();
    public ObservableCollection<Customer> Customers { get; } = new();

    public CustomerViewModel(
        ICustomerRepository customerRepository,
        IDialogService? dialogService = null,
        IValidator<Customer>? customerValidator = null)
    {
        _customerRepository = customerRepository;
        _dialogService = dialogService;
        _customerValidator = customerValidator ?? new CustomerValidator();
        GenerateNewId();
        _ = LoadDataAsync();
    }

    public CustomerViewModel() : this(null!, null, null)
    {
    }

    private async Task LoadDataAsync()
    {
        if (_customerRepository == null) return;

        AllCustomers.Clear();
        var customers = await _customerRepository.GetCustomersAsync();
        foreach (var c in customers) AllCustomers.Add(c);
        FilterCustomers();
        GenerateNewId();
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

    private async Task<bool> ValidateCustomerModelAsync(Customer customer)
    {
        var validationResult = _customerValidator.Validate(customer);
        if (validationResult.IsValid)
        {
            return true;
        }

        if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync(validationResult.Errors[0].ErrorMessage);
        }

        return false;
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນຊື່ລູກຄ້າ");
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerId))
        {
            GenerateNewId();
        }

        var newCustomer = new Customer
        {
            Id = CustomerId,
            Name = Name,
            Surname = Surname,
            Gender = Gender,
            PhoneNumber = PhoneNumber,
            Address = Address
        };

        if (!await ValidateCustomerModelAsync(newCustomer))
        {
            return;
        }

        bool success = await _customerRepository.AddCustomerAsync(newCustomer);
        if (success)
        {
            UpsertCustomer(newCustomer);
            FilterCustomers();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ເພີ່ມລູກຄ້າສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມລູກຄ້າບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedCustomer == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກລູກຄ້າກ່ອນ");
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນຊື່ລູກຄ້າ");
            return;
        }

        var updateCustomer = new Customer
        {
            Id = SelectedCustomer.Id,
            Name = Name,
            Surname = Surname,
            Gender = Gender,
            PhoneNumber = PhoneNumber,
            Address = Address
        };

        if (!await ValidateCustomerModelAsync(updateCustomer))
        {
            return;
        }

        bool success = await _customerRepository.UpdateCustomerAsync(updateCustomer);
        if (success)
        {
            UpsertCustomer(updateCustomer);
            FilterCustomers();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ແກ້ໄຂລູກຄ້າສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ແກ້ໄຂລູກຄ້າບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedCustomer == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກລູກຄ້າກ່ອນ");
            return;
        }

        bool confirm = true;
        if (_dialogService != null)
            confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບລູກຄ້າ {SelectedCustomer.Name} {SelectedCustomer.Surname} ຫຼືບໍ່?");

        if (!confirm) return;

        bool success = await _customerRepository.DeleteCustomerAsync(SelectedCustomer.Id);
        if (success)
        {
            RemoveCustomerById(SelectedCustomer.Id);
            FilterCustomers();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ລຶບລູກຄ້າສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ລຶບລູກຄ້າບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedCustomer = null;
        Name = string.Empty;
        Surname = string.Empty;
        Gender = "ຊາຍ";
        PhoneNumber = string.Empty;
        Address = string.Empty;
        GenerateNewId();
    }

    private void UpsertCustomer(Customer customer)
    {
        for (var i = 0; i < AllCustomers.Count; i++)
        {
            if (AllCustomers[i].Id == customer.Id)
            {
                AllCustomers[i] = customer;
                return;
            }
        }
        AllCustomers.Add(customer);
    }

    private void RemoveCustomerById(string customerId)
    {
        for (var i = 0; i < AllCustomers.Count; i++)
        {
            if (AllCustomers[i].Id == customerId)
            {
                AllCustomers.RemoveAt(i);
                return;
            }
        }
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

        foreach (var customer in query) Customers.Add(customer);
    }
}
