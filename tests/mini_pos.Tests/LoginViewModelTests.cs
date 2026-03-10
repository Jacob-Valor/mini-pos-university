using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

namespace mini_pos.Tests;

public class LoginViewModelTests
{
    [Fact]
    public void ValidateCredentials_EmptyUsername_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("ຊື່ຜູ້ໃຊ້", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_EmptyPassword_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin";
        vm.Password = "";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("ລະຫັດຜ່ານ", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_ShortUsername_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "ab";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("3-50", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_LongUsername_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = new string('a', 51);
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("3-50", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_ShortPassword_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin";
        vm.Password = "123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("4-100", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_LongPassword_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin";
        vm.Password = new string('a', 101);

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("4-100", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_UsernameWithSingleQuote_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin'test";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("ບໍ່ອະນຸຍາດ", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_UsernameWithDoubleQuote_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin\"test";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("ບໍ່ອະນຸຍາດ", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_UsernameWithSemicolon_ReturnsError()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin;test";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.False(result.IsValid);
        Assert.Contains("ບໍ່ອະນຸຍາດ", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCredentials_ValidInput_ReturnsSuccess()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin123";
        vm.Password = "password123";

        var result = CallValidateCredentials(vm);

        Assert.True(result.IsValid);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public void MaskUsername_Empty_ReturnsEmptyMarker()
    {
        var result = LoginViewModelTestsHelper.MaskUsername("");
        Assert.Equal("[empty]", result);
    }

    [Fact]
    public void MaskUsername_Null_ReturnsEmptyMarker()
    {
        var result = LoginViewModelTestsHelper.MaskUsername(null);
        Assert.Equal("[empty]", result);
    }

    [Fact]
    public void MaskUsername_ShortUsername_ReturnsStars()
    {
        var result = LoginViewModelTestsHelper.MaskUsername("ab");
        Assert.Equal("***", result);
    }

    [Fact]
    public void MaskUsername_LongUsername_PartiallyMasked()
    {
        var result = LoginViewModelTestsHelper.MaskUsername("admin");
        Assert.Equal("ad***", result);
    }

    [Fact]
    public void ClearCommand_ResetsAllProperties()
    {
        var vm = CreateLoginViewModel();
        vm.Username = "admin";
        vm.Password = "password";
        vm.ShowPassword = true;
        vm.HasError = true;
        vm.ErrorMessage = "Error";
        vm.IsLoading = true;

        vm.ClearCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Username);
        Assert.Equal(string.Empty, vm.Password);
        Assert.False(vm.ShowPassword);
        Assert.False(vm.HasError);
        Assert.Null(vm.ErrorMessage);
        Assert.False(vm.IsLoading);
    }

    private static LoginViewModel CreateLoginViewModel()
    {
        var mockEmployees = new Mock<IEmployeeRepository>();
        var mockCredentials = new Mock<IEmployeeCredentialsRepository>();
        return new LoginViewModel(mockEmployees.Object, mockCredentials.Object, null);
    }

    private static (bool IsValid, string ErrorMessage) CallValidateCredentials(LoginViewModel vm)
    {
        var method = typeof(LoginViewModel).GetMethod("ValidateCredentials",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return ((bool IsValid, string ErrorMessage))method!.Invoke(vm, null)!;
    }
}

public static class LoginViewModelTestsHelper
{
    public static string MaskUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "[empty]";

        if (username.Length <= 2)
            return "***";

        return username.Substring(0, 2) + new string('*', username.Length - 2);
    }
}
