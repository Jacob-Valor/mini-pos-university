using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using mini_pos.ViewModels;
using ReactiveUI;

namespace mini_pos.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the window is opened. Auto-focuses the username field.
        /// </summary>
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
            usernameTextBox?.Focus();
        }

        /// <summary>
        /// Handles Enter key on username field - moves focus to password field.
        /// </summary>
        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
                var passwordVisibleTextBox = this.FindControl<TextBox>("PasswordVisibleTextBox");

                // Focus the visible password field
                if (passwordTextBox?.IsVisible == true)
                {
                    passwordTextBox.Focus();
                }
                else if (passwordVisibleTextBox?.IsVisible == true)
                {
                    passwordVisibleTextBox.Focus();
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles Enter key on password field - triggers login command.
        /// </summary>
        private void OnPasswordKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.LoginCommand.Execute().Subscribe();
                }
                e.Handled = true;
            }
        }
    }
}
