using ReactiveUI;
using System.Reactive;
using System;

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

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> ClearCommand { get; }

        public event EventHandler? LoginSuccessful;

        public LoginViewModel()
        {
            LoginCommand = ReactiveCommand.Create(Login);
            ClearCommand = ReactiveCommand.Create(Clear);
        }

        private void Login()
        {
            // TODO: Implement actual login logic with database verification
            Console.WriteLine($"Login attempted with username: {Username}");
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }

        private void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
            ShowPassword = false;
        }
    }
}