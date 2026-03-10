using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;

using mini_pos.Models;
using mini_pos.Services;
using mini_pos.ViewModels;
using mini_pos.Views;

using Moq;

using Xunit;

namespace mini_pos.Tests;

public class HeadlessUiSmokeTests
{
    [AvaloniaFact]
    public void LoginView_BindsTextInputsAndLoadingState()
    {
        var viewModel = new LoginViewModel(Mock.Of<IEmployeeRepository>(), Mock.Of<IEmployeeCredentialsRepository>());
        var window = new LoginView { DataContext = viewModel };

        window.Show();

        var usernameTextBox = window.FindControl<TextBox>("UsernameTextBox");
        var passwordTextBox = window.FindControl<TextBox>("PasswordTextBox");
        var loginButton = window.FindControl<Button>("LoginButton");
        var loadingButton = window.FindControl<Button>("LoadingButton");

        Assert.NotNull(usernameTextBox);
        Assert.NotNull(passwordTextBox);
        Assert.NotNull(loginButton);
        Assert.NotNull(loadingButton);

        usernameTextBox!.Text = "admin";
        passwordTextBox!.Text = "secret123";

        Assert.Equal("admin", viewModel.Username);
        Assert.Equal("secret123", viewModel.Password);

        viewModel.IsLoading = true;

        Assert.False(loginButton!.IsVisible);
        Assert.True(loadingButton!.IsVisible);

        window.Close();
    }

    [AvaloniaFact]
    public void MainWindow_ShowsCurrentUserAndSwapsGreetingForCurrentPage()
    {
        var employee = new Employee
        {
            Id = "EMP001",
            Name = "Jane",
            Surname = "Doe",
            Position = "Admin"
        };

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer>());
        var customerPage = new CustomerViewModel(customerRepository.Object);

        var viewModel = new MainWindowViewModel(employee, MockDialogService().Object, MockNavigationService().Object)
        {
            CurrentPage = customerPage
        };

        var window = new MainWindow { DataContext = viewModel };
        window.Show();

        var currentUserTextBlock = window.FindControl<TextBlock>("CurrentUserTextBlock");
        var currentRoleTextBlock = window.FindControl<TextBlock>("CurrentRoleTextBlock");
        var greetingTextBlock = window.FindControl<TextBlock>("GreetingTextBlock");
        var currentPageContent = window.FindControl<TransitioningContentControl>("CurrentPageContent");
        var homeMenuItem = window.FindControl<MenuItem>("HomeMenuItem");
        var customersMenuItem = window.FindControl<MenuItem>("CustomersMenuItem");
        var saleMenuItem = window.FindControl<MenuItem>("SaleMenuItem");
        var profileMenuItem = window.FindControl<MenuItem>("ProfileMenuItem");

        Assert.Equal("Jane Doe", currentUserTextBlock!.Text);
        Assert.Equal("ແອັດມິນ", currentRoleTextBlock!.Text);
        Assert.False(greetingTextBlock!.IsVisible);
        Assert.Same(customerPage, currentPageContent!.Content);
        Assert.NotNull(homeMenuItem);
        Assert.NotNull(customersMenuItem);
        Assert.NotNull(saleMenuItem);
        Assert.NotNull(profileMenuItem);

        viewModel.LogoutCommand.Execute(null);
        window.Close();
    }

    [AvaloniaFact]
    public void SalesView_BindsInputsAndCommandStates()
    {
        var viewModel = CreateSalesViewModel();
        viewModel.CustomerName = "Walk-in";
        viewModel.ProductName = "Rice";
        viewModel.Unit = "bag";
        viewModel.UnitPrice = 120m;
        viewModel.Quantity = 2;

        var view = new SalesView { DataContext = viewModel };
        var hostWindow = new Window { Content = view };

        hostWindow.Show();

        var customerNameTextBox = view.FindControl<TextBox>("CustomerNameTextBox");
        var productNameTextBox = view.FindControl<TextBox>("ProductNameTextBox");
        var unitTextBox = view.FindControl<TextBox>("UnitTextBox");
        var addProductButton = view.FindControl<Button>("AddProductButton");
        var saveSaleButton = view.FindControl<Button>("SaveSaleButton");
        var paymentButton = view.FindControl<Button>("PaymentButton");

        Assert.NotNull(customerNameTextBox);
        Assert.NotNull(productNameTextBox);
        Assert.NotNull(unitTextBox);
        Assert.NotNull(addProductButton);
        Assert.NotNull(saveSaleButton);
        Assert.NotNull(paymentButton);

        Assert.Equal("Walk-in", customerNameTextBox!.Text);
        Assert.Equal("Rice", productNameTextBox!.Text);
        Assert.Equal("bag", unitTextBox!.Text);
        Assert.True(addProductButton!.IsEnabled);
        Assert.False(saveSaleButton!.IsEnabled);
        Assert.False(paymentButton!.IsEnabled);

        customerNameTextBox.Text = "Jane Doe";

        Assert.Equal("Jane Doe", viewModel.CustomerName);

        hostWindow.Close();
    }

    private static SalesViewModel CreateSalesViewModel()
    {
        var employee = new Employee
        {
            Id = "EMP001",
            Name = "Jane",
            Surname = "Doe",
            Position = "Employee"
        };

        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(x => x.GetProductByBarcodeAsync(It.IsAny<string>())).ReturnsAsync((Product?)null);

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(x => x.SearchCustomersAsync(It.IsAny<string>())).ReturnsAsync(new List<Customer>());

        var exchangeRateRepository = new Mock<IExchangeRateRepository>();
        exchangeRateRepository.Setup(x => x.GetLatestExchangeRateAsync()).ReturnsAsync((ExchangeRate?)null);

        var salesRepository = new Mock<ISalesRepository>();
        salesRepository.Setup(x => x.CreateSaleAsync(It.IsAny<Sale>(), It.IsAny<IEnumerable<SaleDetail>>())).ReturnsAsync(true);

        return new SalesViewModel(
            employee,
            productRepository.Object,
            customerRepository.Object,
            exchangeRateRepository.Object,
            salesRepository.Object);
    }

    private static Mock<IDialogService> MockDialogService()
    {
        var dialogService = new Mock<IDialogService>();
        dialogService.Setup(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MsBox.Avalonia.Enums.Icon>())).Returns(Task.CompletedTask);
        dialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        dialogService.Setup(x => x.ShowErrorAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        dialogService.Setup(x => x.ShowSuccessAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        return dialogService;
    }

    private static Mock<INavigationService> MockNavigationService()
    {
        var navigationService = new Mock<INavigationService>();
        navigationService.Setup(x => x.CreateViewModel<CustomerViewModel>()).Returns(new CustomerViewModel(new Mock<ICustomerRepository>().Object));
        return navigationService;
    }
}
