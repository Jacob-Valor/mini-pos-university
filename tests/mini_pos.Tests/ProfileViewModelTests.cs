using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System;

namespace mini_pos.Tests;

public class ProfileViewModelTests
{
    [Fact]
    public void ProfileViewModel_LoadsUserData()
    {
        var employee = new Employee
        {
            Id = "EMP001",
            Username = "john",
            Name = "John",
            Surname = "Doe",
            Gender = "ຊາຍ",
            PhoneNumber = "12345678",
            Province = "Vientiane",
            District = "Saysettha",
            Village = "Village1",
            Position = "Admin"
        };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        Assert.Equal("EMP001", vm.Id);
        Assert.Equal("john", vm.Username);
        Assert.Equal("John", vm.Name);
        Assert.Equal("Doe", vm.Surname);
        Assert.Equal("Admin", vm.Position);
    }

    [Fact]
    public void SaveProfile_CallsRepository()
    {
        var employee = new Employee
        {
            Id = "EMP001",
            Username = "john",
            Name = "John",
            Surname = "Doe"
        };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        mockEmpRepo.Setup(x => x.UpdateEmployeeProfileAsync(It.IsAny<Employee>())).ReturnsAsync(true);

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.Name = "Jane";
        vm.SaveProfileCommand.Execute(null);

        mockEmpRepo.Verify(x => x.UpdateEmployeeProfileAsync(It.Is<Employee>(e => e.Name == "Jane")), Times.Once);
    }

    [Fact]
    public void ChangePassword_EmptyFields_ValidationFails()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.ChangePasswordCommand.Execute(null);
    }

    [Fact]
    public void ChangePassword_PasswordMismatch_ShowsError()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.OldPassword = "oldpassword";
        vm.NewPassword = "newpassword";
        vm.ConfirmPassword = "differentpassword";

        vm.ChangePasswordCommand.Execute(null);
    }

    [Fact]
    public void ChangePassword_WrongOldPassword_ValidationFails()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();
        mockCredRepo.Setup(x => x.GetStoredPasswordHashAsync("john")).ReturnsAsync("somehash");

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.OldPassword = "wrongpassword";
        vm.NewPassword = "newpassword123";
        vm.ConfirmPassword = "newpassword123";

        vm.ChangePasswordCommand.Execute(null);
    }

    [Fact]
    public void ChangePassword_Valid_CallsRepository()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        string storedHash = PasswordHelper.HashPassword("oldpassword");
        mockCredRepo.Setup(x => x.GetStoredPasswordHashAsync("john")).ReturnsAsync(storedHash);
        mockCredRepo.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.OldPassword = "oldpassword";
        vm.NewPassword = "newpassword123";
        vm.ConfirmPassword = "newpassword123";

        vm.ChangePasswordCommand.Execute(null);

        mockCredRepo.Verify(x => x.UpdatePasswordAsync("EMP001", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ChangePassword_Success_ClearsFields()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        string storedHash = PasswordHelper.HashPassword("oldpassword");
        mockCredRepo.Setup(x => x.GetStoredPasswordHashAsync("john")).ReturnsAsync(storedHash);
        mockCredRepo.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        vm.OldPassword = "oldpassword";
        vm.NewPassword = "newpassword123";
        vm.ConfirmPassword = "newpassword123";

        vm.ChangePasswordCommand.Execute(null);

        Assert.Equal("", vm.OldPassword);
        Assert.Equal("", vm.NewPassword);
        Assert.Equal("", vm.ConfirmPassword);
    }

    [Fact]
    public void ProfileViewModel_ContainsDefaultCollections()
    {
        var employee = new Employee { Id = "EMP001", Username = "john", Name = "John" };

        var mockEmpRepo = new Mock<IEmployeeRepository>();
        var mockCredRepo = new Mock<IEmployeeCredentialsRepository>();

        var vm = new ProfileViewModel(employee, mockEmpRepo.Object, mockCredRepo.Object, null);

        Assert.NotEmpty(vm.Provinces);
        Assert.NotEmpty(vm.Districts);
        Assert.NotEmpty(vm.Villages);
        Assert.NotEmpty(vm.Positions);
    }
}
