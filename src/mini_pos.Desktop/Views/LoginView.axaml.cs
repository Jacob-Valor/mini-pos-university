using System;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using mini_pos.ViewModels;

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

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
            usernameTextBox?.Focus();
        }

        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
                var passwordVisibleTextBox = this.FindControl<TextBox>("PasswordVisibleTextBox");

<<<<<<< HEAD:Views/LoginView.axaml.cs
                // Focus the visible password field
=======
>>>>>>> dev:src/mini_pos.Desktop/Views/LoginView.axaml.cs
                if (passwordTextBox?.IsVisible == true)
                    passwordTextBox.Focus();
                else if (passwordVisibleTextBox?.IsVisible == true)
                    passwordVisibleTextBox.Focus();
<<<<<<< HEAD:Views/LoginView.axaml.cs
                }
=======
>>>>>>> dev:src/mini_pos.Desktop/Views/LoginView.axaml.cs

                e.Handled = true;
            }
        }

        private void OnPasswordKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is LoginViewModel vm && vm.LoginCommand.CanExecute(null))
                {
                    vm.LoginCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
