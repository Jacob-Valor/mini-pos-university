using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using mini_pos.Models;
using mini_pos.Services;
using Serilog;

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

    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeCredentialsRepository _employeeCredentialsRepository;
    private readonly IDialogService? _dialogService;

    public LoginViewModel(
        IEmployeeRepository employeeRepository,
        IEmployeeCredentialsRepository employeeCredentialsRepository,
        IDialogService? dialogService = null)
    {
        _employeeRepository = employeeRepository;
        _employeeCredentialsRepository = employeeCredentialsRepository;
        _dialogService = dialogService;
    }

    public LoginViewModel() : this(null!, null!, null)
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
            Log.Information("Login attempt for user {Username}", MaskUsername(Username));

            var storedHash = await _employeeCredentialsRepository.GetStoredPasswordHashAsync(Username ?? string.Empty);

            if (storedHash != null)
            {
                bool isValid = PasswordHelper.VerifyPassword(Password ?? string.Empty, storedHash);

                if (isValid)
                {
                    var employee = await _employeeRepository.GetEmployeeByUsernameAsync(Username ?? string.Empty);
                    if (employee != null)
                    {
                        if (PasswordHelper.NeedsRehash(storedHash))
                        {
                            Log.Information("Upgrading password hash for user {Username}", MaskUsername(Username));
                            var newHash = PasswordHelper.HashPassword(Password ?? string.Empty);
                            var upgraded = await _employeeCredentialsRepository.UpdatePasswordAsync(employee.Id, newHash);
                            if (upgraded)
                                Log.Information("Password hash upgraded for employee {EmployeeId}", employee.Id);
                            else
                                Log.Warning("Password hash upgrade failed for employee {EmployeeId}", employee.Id);
                        }

                        CurrentEmployee = employee;
                        Log.Information("Login successful for employee {EmployeeId}", employee.Id);
                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    Log.Warning("Login verified password but employee lookup failed for user {Username}", MaskUsername(Username));
                }
            }

            HasError = true;
            ErrorMessage = "ຊື່ຜູ້ໃຊ້ ຫຼື ລະຫັດຜ່ານບໍ່ຖືກຕ້ອງ";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
            Log.Warning("Login failed for user {Username}", MaskUsername(Username));
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = "ເກີດຂໍ້ຜິດພາດ ກະລຸນາລອງໃໝ່";
            if (_dialogService != null)
            {
                await _dialogService.ShowErrorAsync(ErrorMessage);
            }
            Log.Error(ex, "Login error for user {Username}", MaskUsername(Username));
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
