using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private bool _showPassword;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isLoading;

    public Employee? CurrentEmployee { get; private set; }

    public event EventHandler? LoginSuccessful;

    private readonly IDatabaseService _databaseService;
    private readonly IDialogService? _dialogService;

    public LoginViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
    }

    public LoginViewModel() : this(null!, null)
    {
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        HasError = false;
        ErrorMessage = null;

        var validationResult = ValidateCredentials();
        if (!validationResult.IsValid)
        {
            HasError = true;
            ErrorMessage = validationResult.ErrorMessage;
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage ?? string.Empty);
            }
            return;
        }

        IsLoading = true;

        try
        {
            Console.WriteLine($"Login attempt for user: {MaskUsername(Username)}");

            var storedHash = await _databaseService.GetStoredPasswordHashAsync(Username ?? string.Empty);

            if (storedHash != null)
            {
                bool isValid = PasswordHelper.VerifyPassword(Password ?? string.Empty, storedHash);

                if (isValid)
                {
                    if (storedHash.Length == 32)
                    {
                        Console.WriteLine("Upgrading legacy MD5 password to PBKDF2...");
                    }

                    var employee = await _databaseService.GetEmployeeByUsernameAsync(Username ?? string.Empty);

                    if (employee != null)
                    {
                        if (storedHash.Length == 32)
                        {
                            var newHash = PasswordHelper.HashPassword(Password ?? string.Empty);
                            await _databaseService.UpdatePasswordAsync(employee.Id, newHash);
                            Console.WriteLine("Password upgraded successfully.");
                        }

                        CurrentEmployee = employee;
                        Console.WriteLine($"Login successful for: {employee.Name} {employee.Surname}");
                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
            }

            HasError = true;
            ErrorMessage = "ຊື່ຜູ້ໃຊ້ ຫຼື ລະຫັດຜ່ານບໍ່ຖືກຕ້ອງ";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
            Console.WriteLine("Login failed: Invalid credentials");
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"ເກີດຂໍ້ຜິດພາດ: {ex.Message}";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
            Console.Error.WriteLine($"Login error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        Username = string.Empty;
        Password = string.Empty;
        ShowPassword = false;
        HasError = false;
        ErrorMessage = null;
        IsLoading = false;
    }

    private (bool IsValid, string ErrorMessage) ValidateCredentials()
    {
        if (string.IsNullOrWhiteSpace(Username))
            return (false, "ກະລຸນາປ້ອນຊື່ຜູ້ໃຊ້");

        if (string.IsNullOrWhiteSpace(Password))
            return (false, "ກະລຸນາປ້ອນລະຫັດຜ່ານ");

        if (Username.Length < 3 || Username.Length > 50)
            return (false, "ຊື່ຜູ້ໃຊ້ຕ້ອງມີ 3-50 ຕົວອັກສອນ");

        if (Password.Length < 4 || Password.Length > 100)
            return (false, "ລະຫັດຜ່ານຕ້ອງມີ 4-100 ຕົວອັກສອນ");

        if (Username.Contains("'") || Username.Contains("\"") || Username.Contains(";"))
            return (false, "ຊື່ຜູ້ໃຊ້ມີອັກສອນທີ່ບໍ່ອະນຸຍາດ");

        return (true, string.Empty);
    }

    private static string MaskUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "[empty]";

        if (username.Length <= 2)
            return "***";

        return username.Substring(0, 2) + new string('*', username.Length - 2);
    }
}
