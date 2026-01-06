using ReactiveUI;
using System.Reactive;
using System;
using System.Security.Cryptography;
using System.Text;
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

        public LoginViewModel()
        {
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        /// <summary>
        /// Computes MD5 hash of the input string.
        /// Returns lowercase 32-character hex string (matches database format).
        /// </summary>
        private static string ComputeMd5Hash(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        private async Task LoginAsync()
        {
            // Clear previous error
            HasError = false;
            ErrorMessage = null;

            // Validate input
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                HasError = true;
                ErrorMessage = "ກະລຸນາປ້ອນຊື່ຜູ້ໃຊ້ ແລະ ລະຫັດຜ່ານ";
                return;
            }

            IsLoading = true;

            try
            {
                // Hash password using MD5 (matches database storage format)
                string hashedPassword = ComputeMd5Hash(Password);
                
                Console.WriteLine($"Login attempted with username: {Username}");

                // Validate credentials against database
                var employee = await DatabaseService.Instance.ValidateLoginAsync(Username, hashedPassword);

                if (employee != null)
                {
                    CurrentEmployee = employee;
                    Console.WriteLine($"Login successful for: {employee.Name} {employee.Surname}");
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "ຊື່ຜູ້ໃຊ້ ຫຼື ລະຫັດຜ່ານບໍ່ຖືກຕ້ອງ";
                    Console.WriteLine("Login failed: Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"ເກີດຂໍ້ຜິດພາດ: {ex.Message}";
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
    }
}
