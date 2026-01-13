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

public class EmployeeViewModel : ViewModelBase
{
    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;
    private bool _suppressLocationUpdates;

    private Employee? _selectedEmployee;
    public Employee? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
            if (value != null)
            {
                EmployeeId = value.Id;
                EmployeeName = value.Name;
                EmployeeSurname = value.Surname;
                EmployeeGender = value.Gender;
                EmployeeDateOfBirth = value.DateOfBirth;
                EmployeePhoneNumber = value.PhoneNumber;
                EmployeePassword = value.Password;
                EmployeeImagePath = value.ImagePath;
                SelectedPosition = value.Position;

                _ = SetLocationFromEmployeeAsync(value);
            }
        }
    }

    private string _employeeId = string.Empty;
    public string EmployeeId
    {
        get => _employeeId;
        set => this.RaiseAndSetIfChanged(ref _employeeId, value);
    }

    private string _employeeName = string.Empty;
    public string EmployeeName
    {
        get => _employeeName;
        set => this.RaiseAndSetIfChanged(ref _employeeName, value);
    }

    private string _employeeSurname = string.Empty;
    public string EmployeeSurname
    {
        get => _employeeSurname;
        set => this.RaiseAndSetIfChanged(ref _employeeSurname, value);
    }

    private string _employeeGender = "ຊາຍ"; // Default Male
    public string EmployeeGender
    {
        get => _employeeGender;
        set => this.RaiseAndSetIfChanged(ref _employeeGender, value);
    }

    private DateTimeOffset _employeeDateOfBirth = DateTimeOffset.Now;
    public DateTimeOffset EmployeeDateOfBirth
    {
        get => _employeeDateOfBirth;
        set => this.RaiseAndSetIfChanged(ref _employeeDateOfBirth, value);
    }

    private string _employeePhoneNumber = string.Empty;
    public string EmployeePhoneNumber
    {
        get => _employeePhoneNumber;
        set => this.RaiseAndSetIfChanged(ref _employeePhoneNumber, value);
    }

    private string _employeePassword = string.Empty;
    public string EmployeePassword
    {
        get => _employeePassword;
        set => this.RaiseAndSetIfChanged(ref _employeePassword, value);
    }

    private string _employeeImagePath = string.Empty;
    public string EmployeeImagePath
    {
        get => _employeeImagePath;
        set => this.RaiseAndSetIfChanged(ref _employeeImagePath, value);
    }

    // Geo-Location Selected Items (Objects)
    private Province? _selectedProvinceItem;
    public Province? SelectedProvinceItem
    {
        get => _selectedProvinceItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedProvinceItem, value);
            if (!_suppressLocationUpdates)
            {
                _ = LoadDistrictsAsync(value?.Id);
            }
        }
    }

    private District? _selectedDistrictItem;
    public District? SelectedDistrictItem
    {
        get => _selectedDistrictItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDistrictItem, value);
            if (!_suppressLocationUpdates)
            {
                _ = LoadVillagesAsync(value?.Id);
            }
        }
    }

    private Village? _selectedVillageItem;
    public Village? SelectedVillageItem
    {
        get => _selectedVillageItem;
        set => this.RaiseAndSetIfChanged(ref _selectedVillageItem, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            this.RaiseAndSetIfChanged(ref _searchText, value);
            FilterEmployees();
        }
    }

    private string? _selectedPosition;
    public string? SelectedPosition
    {
        get => _selectedPosition;
        set => this.RaiseAndSetIfChanged(ref _selectedPosition, value);
    }

    public ObservableCollection<Employee> AllEmployees { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();

    // Data Sources
    public ObservableCollection<Province> Provinces { get; } = new();
    public ObservableCollection<District> Districts { get; } = new();
    public ObservableCollection<Village> Villages { get; } = new();
    public ObservableCollection<string> Positions { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public EmployeeViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;

        Positions.Add("Admin");
        Positions.Add("Employee");

        var canAdd = this.WhenAnyValue(x => x.EmployeeId, x => x.EmployeeName, x => x.SelectedVillageItem)
            .Select(t => !string.IsNullOrWhiteSpace(t.Item1) && 
                         !string.IsNullOrWhiteSpace(t.Item2) && 
                         t.Item3 != null);

        AddCommand = ReactiveCommand.CreateFromTask(AddAsync, canAdd);

        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedEmployee)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.CreateFromTask(EditAsync, canEditOrDelete);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);

        _ = LoadInitialDataAsync();
    }

    public EmployeeViewModel() : this(null!, null)
    {
        // Design-time
    }

    private async Task LoadInitialDataAsync()
    {
        if (_databaseService == null) return;

        // Load Provinces
        Provinces.Clear();
        var provs = await _databaseService.GetProvincesAsync();
        foreach (var p in provs) Provinces.Add(p);

        // Load Employees
        await RefreshEmployeeList();
    }

    private async Task SetLocationFromEmployeeAsync(Employee employee)
    {
        _suppressLocationUpdates = true;
        SelectedProvinceItem = Provinces.FirstOrDefault(p => p.Id == employee.ProvinceId) ??
                               Provinces.FirstOrDefault(p => p.Name == employee.Province);
        _suppressLocationUpdates = false;

        await LoadDistrictsAsync(SelectedProvinceItem?.Id);

        _suppressLocationUpdates = true;
        SelectedDistrictItem = Districts.FirstOrDefault(d => d.Id == employee.DistrictId) ??
                               Districts.FirstOrDefault(d => d.Name == employee.District);
        _suppressLocationUpdates = false;

        await LoadVillagesAsync(SelectedDistrictItem?.Id);

        _suppressLocationUpdates = true;
        SelectedVillageItem = Villages.FirstOrDefault(v => v.Id == employee.VillageId) ??
                              Villages.FirstOrDefault(v => v.Name == employee.Village);
        _suppressLocationUpdates = false;
    }

    private async Task RefreshEmployeeList()
    {
        AllEmployees.Clear();
        var emps = await _databaseService.GetEmployeesAsync();
        foreach (var e in emps) AllEmployees.Add(e);
        FilterEmployees();
    }

    private async Task LoadDistrictsAsync(string? provinceId)
    {
        Districts.Clear();
        if (string.IsNullOrEmpty(provinceId)) return;
        
        var dists = await _databaseService.GetDistrictsByProvinceAsync(provinceId);
        foreach (var d in dists) Districts.Add(d);
    }

    private async Task LoadVillagesAsync(string? districtId)
    {
        Villages.Clear();
        if (string.IsNullOrEmpty(districtId)) return;

        var vils = await _databaseService.GetVillagesByDistrictAsync(districtId);
        foreach (var v in vils) Villages.Add(v);
    }

    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeId) || string.IsNullOrWhiteSpace(EmployeeName)) return;

        var newEmployee = new Employee
        {
            Id = EmployeeId,
            Name = EmployeeName,
            Surname = EmployeeSurname,
            Gender = EmployeeGender,
            DateOfBirth = EmployeeDateOfBirth.DateTime,
            PhoneNumber = EmployeePhoneNumber,
            Province = SelectedProvinceItem?.Name ?? string.Empty,
            District = SelectedDistrictItem?.Name ?? string.Empty,
            Village = SelectedVillageItem?.Name ?? string.Empty,
            ProvinceId = SelectedProvinceItem?.Id ?? string.Empty,
            DistrictId = SelectedDistrictItem?.Id ?? string.Empty,
            VillageId = SelectedVillageItem?.Id ?? string.Empty,
            Username = EmployeeId,
            Password = PasswordHelper.HashPassword(EmployeePassword),
            Position = SelectedPosition ?? "Employee",
            ImagePath = EmployeeImagePath
        };

        bool success = await _databaseService.AddEmployeeAsync(newEmployee);
        if (success)
        {
            await RefreshEmployeeList();
            Cancel();
            if (_dialogService != null)
            {
                await _dialogService.ShowSuccessAsync("ເພີ່ມພະນັກງານສຳເລັດ (Employee added)");
            }
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມພະນັກງານບໍ່ສຳເລັດ (Failed to add employee)");
        }
    }

    private async Task EditAsync()
    {
        if (SelectedEmployee != null)
        {
            var provinceId = SelectedProvinceItem?.Id ?? SelectedEmployee.ProvinceId;
            var districtId = SelectedDistrictItem?.Id ?? SelectedEmployee.DistrictId;
            var villageId = SelectedVillageItem?.Id ?? SelectedEmployee.VillageId;

            var updatedEmployee = new Employee
            {
                Id = SelectedEmployee.Id,
                Name = EmployeeName,
                Surname = EmployeeSurname,
                Gender = EmployeeGender,
                DateOfBirth = EmployeeDateOfBirth.DateTime,
                PhoneNumber = EmployeePhoneNumber,
                Province = SelectedProvinceItem?.Name ?? SelectedEmployee.Province,
                District = SelectedDistrictItem?.Name ?? SelectedEmployee.District,
                Village = SelectedVillageItem?.Name ?? SelectedEmployee.Village,
                ProvinceId = provinceId,
                DistrictId = districtId,
                VillageId = villageId,
                Position = SelectedPosition ?? string.Empty,
                Username = SelectedEmployee.Username
            };
            
            bool success = await _databaseService.UpdateEmployeeAsync(updatedEmployee);
            if (success)
            {
                await RefreshEmployeeList();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ແກ້ໄຂພະນັກງານສຳເລັດ (Employee updated)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ແກ້ໄຂພະນັກງານບໍ່ສຳເລັດ (Failed to update employee)");
            }
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedEmployee != null)
        {
            bool confirm = true;
            if (_dialogService != null)
            {
                confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບພະນັກງານ {SelectedEmployee.Name} {SelectedEmployee.Surname} ຫຼືບໍ່?");
            }

            if (!confirm) return;

            bool success = await _databaseService.DeleteEmployeeAsync(SelectedEmployee.Id);
            if (success)
            {
                await RefreshEmployeeList();
                Cancel();
                if (_dialogService != null)
                {
                    await _dialogService.ShowSuccessAsync("ລຶບພະນັກງານສຳເລັດ (Employee deleted)");
                }
            }
            else if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync("ລຶບພະນັກງານບໍ່ສຳເລັດ (Failed to delete employee)");
            }
        }
    }

    private void Cancel()
    {
        SelectedEmployee = null;
        EmployeeId = string.Empty;
        EmployeeName = string.Empty;
        EmployeeSurname = string.Empty;
        EmployeeGender = "ຊາຍ";
        EmployeeDateOfBirth = DateTimeOffset.Now;
        EmployeePhoneNumber = string.Empty;
        EmployeePassword = string.Empty;
        EmployeeImagePath = string.Empty;
        
        SelectedProvinceItem = null;
        SelectedDistrictItem = null;
        SelectedVillageItem = null;
        SelectedPosition = null;
    }

    private void FilterEmployees()
    {
        Employees.Clear();
        var query = AllEmployees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(e =>
                e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Surname.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Id.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var e in query)
        {
            Employees.Add(e);
        }
    }
}
