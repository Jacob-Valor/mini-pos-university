using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using mini_pos.Models;
using ReactiveUI;

namespace mini_pos.ViewModels;

public class EmployeeViewModel : ViewModelBase
{
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

                SelectedProvince = value.Province;
                SelectedDistrict = value.District;
                SelectedVillage = value.Village;
                SelectedPosition = value.Position;
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

    private string? _selectedProvince;
    public string? SelectedProvince
    {
        get => _selectedProvince;
        set => this.RaiseAndSetIfChanged(ref _selectedProvince, value);
    }

    private string? _selectedDistrict;
    public string? SelectedDistrict
    {
        get => _selectedDistrict;
        set => this.RaiseAndSetIfChanged(ref _selectedDistrict, value);
    }

    private string? _selectedVillage;
    public string? SelectedVillage
    {
        get => _selectedVillage;
        set => this.RaiseAndSetIfChanged(ref _selectedVillage, value);
    }

    private string? _selectedPosition;
    public string? SelectedPosition
    {
        get => _selectedPosition;
        set => this.RaiseAndSetIfChanged(ref _selectedPosition, value);
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

    public ObservableCollection<Employee> AllEmployees { get; } = new();
    public ObservableCollection<Employee> Employees { get; } = new();

    // Mock Data Sources
    public ObservableCollection<string> Provinces { get; } = new();
    public ObservableCollection<string> Districts { get; } = new();
    public ObservableCollection<string> Villages { get; } = new();
    public ObservableCollection<string> Positions { get; } = new();

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> EditCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    // public ReactiveCommand<Unit, Unit> UploadImageCommand { get; }

    public EmployeeViewModel()
    {
        // Mock Reference Data
        Provinces.Add("ນະຄອນຫຼວງວຽງຈັນ");
        Provinces.Add("ວຽງຈັນ");
        Provinces.Add("ຫຼວງພະບາງ");

        Districts.Add("ສີສັດຕະນາກ");
        Districts.Add("ໄຊເສດຖາ");
        Districts.Add("ຈັນທະບູລີ");

        Villages.Add("ວັດນາກ");
        Villages.Add("ທາດຂາວ");
        Villages.Add("ທົ່ງພານທອງ");

        Positions.Add("Admin");
        Positions.Add("Employee");

        // Mock Employee Data
        AllEmployees.Add(new Employee
        {
            Id = "EMP00001",
            Name = "ສຸກສະຫວັນ",
            Surname = "ຈຸນລາລີ",
            Gender = "ຊາຍ",
            DateOfBirth = new DateTime(1981, 08, 29),
            PhoneNumber = "96887222",
            Province = "ວຽງຈັນ",
            District = "ທຸລະຄົມ",
            Village = "ບຸ່ງກ້າວ",
            Password = "",
            Position = "Admin",
            ImagePath = "" 
        });

        AllEmployees.Add(new Employee
        {
            Id = "EMP00002",
            Name = "ພຸດທະວີ",
            Surname = "ວົງສາລີ",
            Gender = "ຍິງ",
            DateOfBirth = new DateTime(2003, 05, 11),
            PhoneNumber = "12345678",
            Province = "ນະຄອນຫຼວງວຽງຈັນ",
            District = "ສີສັດຕະນາກ",
            Village = "ວັດນາກ",
            Password = "",
            Position = "Employee",
            ImagePath = "" 
        });

        FilterEmployees();

        AddCommand = ReactiveCommand.Create(Add);
        
        var canEditOrDelete = this.WhenAnyValue(x => x.SelectedEmployee)
                                  .Select(x => x != null);

        EditCommand = ReactiveCommand.Create(Edit, canEditOrDelete);
        DeleteCommand = ReactiveCommand.Create(Delete, canEditOrDelete);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Add()
    {
        if (string.IsNullOrWhiteSpace(EmployeeId) || string.IsNullOrWhiteSpace(EmployeeName)) return;

        var newEmployee = new Employee
        {
            Id = EmployeeId,
            Name = EmployeeName,
            Surname = EmployeeSurname,
            Gender = EmployeeGender,
            DateOfBirth = EmployeeDateOfBirth.DateTime, // Convert back to DateTime
            PhoneNumber = EmployeePhoneNumber,
            Province = SelectedProvince ?? "",
            District = SelectedDistrict ?? "",
            Village = SelectedVillage ?? "",
            Password = EmployeePassword,
            Position = SelectedPosition ?? "",
            ImagePath = EmployeeImagePath
        };

        AllEmployees.Add(newEmployee);
        FilterEmployees();
        Cancel();
    }

    private void Edit()
    {
        if (SelectedEmployee != null)
        {
            var index = AllEmployees.IndexOf(SelectedEmployee);
            if (index != -1)
            {
                var updatedEmployee = new Employee
                {
                    Id = EmployeeId,
                    Name = EmployeeName,
                    Surname = EmployeeSurname,
                    Gender = EmployeeGender,
                    DateOfBirth = EmployeeDateOfBirth.DateTime,
                    PhoneNumber = EmployeePhoneNumber,
                    Province = SelectedProvince ?? "",
                    District = SelectedDistrict ?? "",
                    Village = SelectedVillage ?? "",
                    Password = EmployeePassword,
                    Position = SelectedPosition ?? "",
                    ImagePath = EmployeeImagePath
                };
                AllEmployees[index] = updatedEmployee;
            }
            FilterEmployees();
            Cancel();
        }
    }

    private void Delete()
    {
        if (SelectedEmployee != null)
        {
            AllEmployees.Remove(SelectedEmployee);
            FilterEmployees();
            Cancel();
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
        SelectedProvince = null;
        SelectedDistrict = null;
        SelectedVillage = null;
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
