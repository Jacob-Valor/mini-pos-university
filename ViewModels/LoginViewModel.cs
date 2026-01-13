using ReactiveUI;
using System.Reactive;
using System;
using System.Threading.Tasks;
using mini_pos.Services;
using mini_pos.Models;

namespace mini_pos.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string? _username;
        public string? Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        private bool _showPassword;
        public bool ShowPassword
        {
            get => _showPassword;
            set => this.RaiseAndSetIfChanged(ref _showPassword, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        /// <summary>
        /// The currently logged in employee (set after successful login).
        /// </summary>
        public Employee? CurrentEmployee { get; private set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearCommand { get; }

        public event EventHandler? LoginSuccessful;

        private readonly IDatabaseService _databaseService;
        private readonly IDialogService? _dialogService;

        public LoginViewModel(IDatabaseService databaseService, IDialogService? dialogService = null)
        {
            _databaseService = databaseService;
            _dialogService = dialogService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        public LoginViewModel() : this(null!, null)
        {
            // Design-time constructor
            // In a real scenario we might want a MockDatabaseService here
        }

        private async Task LoginAsync()
        {
            // Clear previous error
            HasError = false;
            ErrorMessage = null;

            // Validate input
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
                // Fetch the stored hash securely
                Console.WriteLine($"Login attempt for user: {MaskUsername(Username)}");
                
                var storedHash = await _databaseService.GetStoredPasswordHashAsync(Username ?? string.Empty);
                
                if (storedHash != null)
                {
                    // Use helper to verify (handles both MD5 and PBKDF2)
                    bool isValid = PasswordHelper.VerifyPassword(Password ?? string.Empty, storedHash);
                    
                    if (isValid)
                    {
                        // Check if we need to upgrade from MD5 to PBKDF2
                        if (storedHash.Length == 32)
                        {
                            Console.WriteLine("Upgrading legacy MD5 password to PBKDF2...");
                        }

                        var employee = await _databaseService.GetEmployeeByUsernameAsync(Username ?? string.Empty);

                        if (employee != null)
                        {
                            // If upgrade needed, do it now that we have the ID
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

                // Fallback for failed login
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

        private void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
            ShowPassword = false;
            HasError = false;
            ErrorMessage = null;
            IsLoading = false;
        }
        
        /// <summary>
        /// Validates username and password input.
        /// </summary>
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
                
            // Check for potentially dangerous characters
            if (Username.Contains("'") || Username.Contains("\"") || Username.Contains(";"))
                return (false, "ຊື່ຜູ້ໃຊ້ມີອັກສອນທີ່ບໍ່ອະນຸຍາດ");
                
            return (true, string.Empty);
        }
        
        /// <summary>
        /// Masks username for secure logging.
        /// </summary>
        private static string MaskUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "[empty]";
                
            if (username.Length <= 2)
                return "***";
                
            return username.Substring(0, 2) + new string('*', username.Length - 2);
        }
    }
}
