using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class EmployeeViewModel : ViewModelBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeCredentialsRepository _employeeCredentialsRepository;
    private readonly IGeoRepository _geoRepository;
    private readonly IDialogService? _dialogService;
    private bool _suppressLocationUpdates;

    [ObservableProperty]
    private Employee? _selectedEmployee;

    partial void OnSelectedEmployeeChanged(Employee? value)
    {
        if (value != null)
        {
            EmployeeId = value.Id;
            EmployeeName = value.Name;
            EmployeeSurname = value.Surname;
            EmployeeGender = value.Gender;
            EmployeeDateOfBirth = value.DateOfBirth;
            EmployeePhoneNumber = value.PhoneNumber;
            EmployeePassword = string.Empty;
            EmployeeImagePath = value.ImagePath;
            SelectedPosition = value.Position;
            _ = SetLocationFromEmployeeAsync(value);
        }
        CanEditOrDelete = value != null;
    }

    [ObservableProperty]
    private string _employeeId = string.Empty;

    [ObservableProperty]
    private string _employeeName = string.Empty;

    [ObservableProperty]
    private string _employeeSurname = string.Empty;

    [ObservableProperty]
    private string _employeeGender = "ຊາຍ";

    [ObservableProperty]
    private DateTimeOffset _employeeDateOfBirth = DateTimeOffset.Now;

    [ObservableProperty]
    private string _employeePhoneNumber = string.Empty;

    [ObservableProperty]
    private string _employeePassword = string.Empty;

    [ObservableProperty]
    private string _employeeImagePath = string.Empty;

    [ObservableProperty]
    private Province? _selectedProvinceItem;

    partial void OnSelectedProvinceItemChanged(Province? value)
    {
        if (!_suppressLocationUpdates) _ = LoadDistrictsAsync(value?.Id);
    }

    [ObservableProperty]
    private District? _selectedDistrictItem;

    partial void OnSelectedDistrictItemChanged(District? value)
    {
        if (!_suppressLocationUpdates) _ = LoadVillagesAsync(value?.Id);
    }

    [ObservableProperty]
    private Village? _selectedVillageItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value) => FilterEmployees();

    [ObservableProperty]
    private string? _selectedPosition;

    [ObservableProperty]
    private bool _canAdd;

    partial void OnEmployeeIdChanged(string value) => UpdateCanAdd();
    partial void OnEmployeeNameChanged(string value) => UpdateCanAdd();
    partial void OnSelectedVillageItemChanged(Village? value) => UpdateCanAdd();

    private void UpdateCanAdd() => CanAdd = !string.IsNullOrWhiteSpace(EmployeeId) &&
                                             !string.IsNullOrWhiteSpace(EmployeeName) &&
                                             SelectedVillageItem != null;

    [ObservableProperty]
    private bool _canEditOrDelete;

    public ObservableCollection<Employee> AllEmployees { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();
    public ObservableCollection<Province> Provinces { get; } = new();
    public ObservableCollection<District> Districts { get; } = new();
    public ObservableCollection<Village> Villages { get; } = new();
    public ObservableCollection<string> Positions { get; } = new();

    public EmployeeViewModel(
        IEmployeeRepository employeeRepository,
        IEmployeeCredentialsRepository employeeCredentialsRepository,
        IGeoRepository geoRepository,
        IDialogService? dialogService = null)
    {
        _employeeRepository = employeeRepository;
        _employeeCredentialsRepository = employeeCredentialsRepository;
        _geoRepository = geoRepository;
        _dialogService = dialogService;
        Positions.Add("Admin");
        Positions.Add("Employee");
        _ = LoadInitialDataAsync();
    }

    public EmployeeViewModel() : this(null!, null!, null!, null)
    {
    }

    private async Task LoadInitialDataAsync()
    {
        if (_employeeRepository == null || _employeeCredentialsRepository == null || _geoRepository == null) return;

        Provinces.Clear();
        var provs = await _geoRepository.GetProvincesAsync();
        foreach (var p in provs) Provinces.Add(p);

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
        var emps = await _employeeRepository.GetEmployeesAsync();
        foreach (var e in emps) AllEmployees.Add(e);
        FilterEmployees();
    }

    private async Task LoadDistrictsAsync(string? provinceId)
    {
        Districts.Clear();
        if (string.IsNullOrEmpty(provinceId)) return;

        var dists = await _geoRepository.GetDistrictsByProvinceAsync(provinceId);
        foreach (var d in dists) Districts.Add(d);
    }

    private async Task LoadVillagesAsync(string? districtId)
    {
        Villages.Clear();
        if (string.IsNullOrEmpty(districtId)) return;

        var vils = await _geoRepository.GetVillagesByDistrictAsync(districtId);
        foreach (var v in vils) Villages.Add(v);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeId) || string.IsNullOrWhiteSpace(EmployeeName) || SelectedVillageItem == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນລະຫັດ, ຊື່ ແລະ ເລືອກບ້ານ");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmployeePassword))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາປ້ອນລະຫັດຜ່ານ");
            return;
        }

        if (EmployeePassword.Length < 4 || EmployeePassword.Length > 100)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ລະຫັດຜ່ານຕ້ອງມີ 4-100 ຕົວອັກສອນ");
            return;
        }

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

        bool success = await _employeeRepository.AddEmployeeAsync(newEmployee);
        if (success)
        {
            await RefreshEmployeeList();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ເພີ່ມພະນັກງານສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ເພີ່ມພະນັກງານບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedEmployee == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກພະນັກງານກ່ອນ");
            return;
        }

        if (!string.IsNullOrWhiteSpace(EmployeePassword) && (EmployeePassword.Length < 4 || EmployeePassword.Length > 100))
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ລະຫັດຜ່ານຕ້ອງມີ 4-100 ຕົວອັກສອນ");
            return;
        }

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

        bool success = await _employeeRepository.UpdateEmployeeAsync(updatedEmployee);
        if (!success)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ແກ້ໄຂພະນັກງານບໍ່ສຳເລັດ");
            return;
        }

        bool passwordUpdated = true;
        if (!string.IsNullOrWhiteSpace(EmployeePassword))
        {
            string newHash = PasswordHelper.HashPassword(EmployeePassword);
            passwordUpdated = await _employeeCredentialsRepository.UpdatePasswordAsync(updatedEmployee.Id, newHash);
        }

        await RefreshEmployeeList();
        Cancel();

        if (_dialogService != null)
        {
            if (passwordUpdated)
                await _dialogService.ShowSuccessAsync("ແກ້ໄຂພະນັກງານສຳເລັດ");
            else
                await _dialogService.ShowErrorAsync("ແກ້ໄຂພະນັກງານສຳເລັດ ແຕ່ ປ່ຽນລະຫັດຜ່ານບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedEmployee == null)
        {
            if (_dialogService != null)
                await _dialogService.ShowErrorAsync("ກະລຸນາເລືອກພະນັກງານກ່ອນ");
            return;
        }

        bool confirm = true;
        if (_dialogService != null)
            confirm = await _dialogService.ShowConfirmationAsync("ຢືນຢັນການລຶບ", $"ລຶບພະນັກງານ {SelectedEmployee.Name} {SelectedEmployee.Surname} ຫຼືບໍ່?");

        if (!confirm) return;

        bool success = await _employeeRepository.DeleteEmployeeAsync(SelectedEmployee.Id);
        if (success)
        {
            await RefreshEmployeeList();
            Cancel();
            if (_dialogService != null)
                await _dialogService.ShowSuccessAsync("ລຶບພະນັກງານສຳເລັດ");
        }
        else if (_dialogService != null)
        {
            await _dialogService.ShowErrorAsync("ລຶບພະນັກງານບໍ່ສຳເລັດ");
        }
    }

    [RelayCommand]
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

        foreach (var e in query) Employees.Add(e);
    }
}
