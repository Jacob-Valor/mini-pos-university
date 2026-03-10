using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mini_pos.Tests;

public class EmployeeViewModelTests
{
    [Fact]
    public void AddEmployee_MissingRequiredFields_ShowsValidationError()
    {
        var vm = CreateEmployeeViewModel();

        vm.EmployeeId = "";
        vm.EmployeeName = "";

        vm.AddCommand.Execute(null);
    }

    [Fact]
    public void AddEmployee_ValidEmployee_CallsRepository()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province>());
        mockEmpRepo.Setup(x => x.AddEmployeeAsync(It.IsAny<Employee>())).ReturnsAsync(true);
        mockEmpRepo.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(new List<Employee>());

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);

        vm.EmployeeId = "EMP001";
        vm.EmployeeName = "John";
        vm.SelectedVillageItem = new Village { Id = "V001", Name = "Village1" };
        vm.SelectedProvinceItem = new Province { Id = "P001", Name = "Province1" };
        vm.SelectedDistrictItem = new District { Id = "D001", Name = "District1" };
        vm.EmployeePassword = "password123";

        vm.AddCommand.Execute(null);

        mockEmpRepo.Verify(x => x.AddEmployeeAsync(It.Is<Employee>(e => e.Id == "EMP001")), Times.Once);
    }

    [Fact]
    public void AddEmployee_PasswordTooShort_ShowsValidationError()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province>());

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);

        vm.EmployeeId = "EMP001";
        vm.EmployeeName = "John";
        vm.SelectedVillageItem = new Village { Id = "V001", Name = "Village1" };
        vm.EmployeePassword = "123";

        vm.AddCommand.Execute(null);

        mockEmpRepo.Verify(x => x.AddEmployeeAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public void EditEmployee_ValidEmployee_CallsRepository()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        var existingEmployee = new Employee
        {
            Id = "EMP001",
            Name = "Old Name",
            Surname = "Doe",
            Gender = "ຊາຍ",
            ProvinceId = "P001",
            Province = "Province1",
            DistrictId = "D001",
            District = "District1",
            VillageId = "V001",
            Village = "Village1"
        };

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province> { new Province { Id = "P001", Name = "Province1" } });
        mockGeoRepo.Setup(x => x.GetDistrictsByProvinceAsync("P001")).ReturnsAsync(new List<District> { new District { Id = "D001", Name = "District1" } });
        mockGeoRepo.Setup(x => x.GetVillagesByDistrictAsync("D001")).ReturnsAsync(new List<Village> { new Village { Id = "V001", Name = "Village1" } });
        mockEmpRepo.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(new List<Employee> { existingEmployee });
        mockEmpRepo.Setup(x => x.UpdateEmployeeAsync(It.IsAny<Employee>())).ReturnsAsync(true);

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);
        vm.SelectedEmployee = vm.AllEmployees.FirstOrDefault();

        vm.EmployeeName = "New Name";
        vm.EditCommand.Execute(null);

        mockEmpRepo.Verify(x => x.UpdateEmployeeAsync(It.Is<Employee>(e => e.Name == "New Name")), Times.Once);
    }

    [Fact]
    public void EditEmployee_WithNewPassword_UpdatesPassword()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        var existingEmployee = new Employee
        {
            Id = "EMP001",
            Name = "John",
            ProvinceId = "P001",
            Province = "Province1",
            DistrictId = "D001",
            District = "District1",
            VillageId = "V001",
            Village = "Village1"
        };

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province> { new Province { Id = "P001", Name = "Province1" } });
        mockGeoRepo.Setup(x => x.GetDistrictsByProvinceAsync("P001")).ReturnsAsync(new List<District> { new District { Id = "D001", Name = "District1" } });
        mockGeoRepo.Setup(x => x.GetVillagesByDistrictAsync("D001")).ReturnsAsync(new List<Village> { new Village { Id = "V001", Name = "Village1" } });
        mockEmpRepo.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(new List<Employee> { existingEmployee });
        mockEmpRepo.Setup(x => x.UpdateEmployeeAsync(It.IsAny<Employee>())).ReturnsAsync(true);
        mockCredRepo.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);
        vm.SelectedEmployee = vm.AllEmployees.FirstOrDefault();

        vm.EmployeePassword = "newpassword123";
        vm.EditCommand.Execute(null);

        mockCredRepo.Verify(x => x.UpdatePasswordAsync("EMP001", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void DeleteEmployee_CallsRepository()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        var existingEmployee = new Employee
        {
            Id = "EMP001",
            Name = "John",
            ProvinceId = "P001",
            Province = "Province1"
        };

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province>());
        mockEmpRepo.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(new List<Employee> { existingEmployee });
        mockEmpRepo.Setup(x => x.DeleteEmployeeAsync("EMP001")).ReturnsAsync(true);

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);
        vm.SelectedEmployee = vm.AllEmployees.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockEmpRepo.Verify(x => x.DeleteEmployeeAsync("EMP001"), Times.Once);
    }

    [Fact]
    public void Cancel_ResetsAllFields()
    {
        var vm = CreateEmployeeViewModel();

        vm.EmployeeId = "EMP001";
        vm.EmployeeName = "John";
        vm.EmployeeSurname = "Doe";
        vm.EmployeePhoneNumber = "12345678";
        vm.SelectedPosition = "Admin";

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.EmployeeId);
        Assert.Equal(string.Empty, vm.EmployeeName);
        Assert.Equal(string.Empty, vm.EmployeeSurname);
        Assert.Equal(string.Empty, vm.EmployeePhoneNumber);
        Assert.Null(vm.SelectedPosition);
        Assert.Null(vm.SelectedEmployee);
    }

    [Fact]
    public void FilterEmployees_ByName_FiltersCorrectly()
    {
        var vm = CreateEmployeeViewModel();

        vm.SearchText = "John";

        Assert.Single(vm.Employees);
        Assert.Equal("John", vm.Employees.First().Name);
    }

    [Fact]
    public void FilterEmployees_BySurname_FiltersCorrectly()
    {
        var vm = CreateEmployeeViewModel();

        vm.SearchText = "Doe";

        Assert.Single(vm.Employees);
        Assert.Equal("Doe", vm.Employees.First().Surname);
    }

    [Fact]
    public void FilterEmployees_EmptySearch_ReturnsAll()
    {
        var vm = CreateEmployeeViewModel();

        vm.SearchText = "";

        Assert.Equal(2, vm.Employees.Count);
    }

    [Fact]
    public void OnSelectedEmployeeChanged_PopulatesFields()
    {
        var vm = CreateEmployeeViewModel();

        vm.SelectedEmployee = vm.AllEmployees.FirstOrDefault();

        Assert.Equal("EMP001", vm.EmployeeId);
        Assert.Equal("John", vm.EmployeeName);
        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedEmployeeNull_CanEditOrDeleteIsFalse()
    {
        var vm = CreateEmployeeViewModel();

        vm.SelectedEmployee = null;

        Assert.False(vm.CanEditOrDelete);
    }

    [Fact]
    public void CanAdd_BecomesTrueWhenAllRequiredFieldsSet()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province>());

        var vm = new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);

        Assert.False(vm.CanAdd);

        vm.EmployeeId = "EMP001";
        Assert.False(vm.CanAdd);

        vm.EmployeeName = "John";
        Assert.False(vm.CanAdd);

        vm.SelectedVillageItem = new Village { Id = "V001", Name = "Village" };
        Assert.True(vm.CanAdd);
    }

    [Fact]
    public void Positions_ContainsDefaultValues()
    {
        var vm = CreateEmployeeViewModel();

        Assert.Contains("Admin", vm.Positions);
        Assert.Contains("Employee", vm.Positions);
    }

    private static EmployeeViewModel CreateEmployeeViewModel()
    {
        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        var mockGeoRepo = new Mock<IGeoRepository>();

        var employees = new List<Employee>
        {
            new Employee { Id = "EMP001", Name = "John", Surname = "Doe", ProvinceId = "P001" },
            new Employee { Id = "EMP002", Name = "Jane", Surname = "Smith", ProvinceId = "P001" }
        };

        mockGeoRepo.Setup(x => x.GetProvincesAsync()).ReturnsAsync(new List<Province>());
        mockEmpRepo.Setup(x => x.GetEmployeesAsync()).ReturnsAsync(employees);

        return new EmployeeViewModel(mockEmpRepo.Object, mockCredRepo.Object, mockGeoRepo.Object, null);
    }
}
