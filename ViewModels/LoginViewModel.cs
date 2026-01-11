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

        private readonly IDatabaseService _databaseService;

        public LoginViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        public LoginViewModel() : this(null!) 
        {
            // Design-time constructor
            // In a real scenario we might want a MockDatabaseService here
        }

        /// <summary>
        /// Computes secure hash of the input string using PBKDF2.
        /// Returns hex string with salt for secure password storage.
        /// </summary>
        private static string ComputeSecureHash(string input)
        {
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            
            // Derive key using PBKDF2 with 10000 iterations
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, 10000, HashAlgorithmName.SHA256, 32);
            
            // Combine salt and hash
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);
            
            return Convert.ToHexString(hashBytes).ToLower();
        }
        
        /// <summary>
        /// Computes MD5 hash for backward compatibility with existing database.
        /// Use ComputeSecureHash for new password storage.
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
            var validationResult = ValidateCredentials();
            if (!validationResult.IsValid)
            {
                HasError = true;
                ErrorMessage = validationResult.ErrorMessage;
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
                            var newHash = PasswordHelper.HashPassword(Password ?? string.Empty);
                            // We need employee ID to update. Fetch employee first.
                            // However, ValidateLoginAsync fetches employee.
                            // Let's just continue to ValidateLoginAsync but PASS THE STORED HASH (MD5) so the query works
                            // The query is: WHERE username = @username AND password = @password
                            // So we must pass the OLD hash to get the user object.
                        }

                        // Validate against DB to get the Employee object
                        // We pass the HASH that is currently in the DB (whether MD5 or PBKDF2)
                        // If we passed the raw password, it would fail.
                        // If we calculated MD5 locally but DB has PBKDF2, it would fail.
                        // So we rely on storedHash which we just verified matches the input.
                        
                        var employee = await _databaseService.ValidateLoginAsync(Username ?? string.Empty, storedHash);

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
                Console.WriteLine("Login failed: Invalid credentials");
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
