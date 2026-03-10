using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Headless.XUnit;

using mini_pos.Models;
using mini_pos.Services;
using mini_pos.ViewModels;

using Moq;

using Xunit;

namespace mini_pos.Tests;

public class MainWindowViewModelTests
{
    [AvaloniaFact]
    public void Constructor_WithAdminEmployee_SetsUserAndTranslatedRole()
    {
        var employee = CreateEmployee(position: "Admin");
        var viewModel = CreateViewModel(employee, out _, out _);

        Assert.Equal("Jane Doe", viewModel.CurrentUser);
        Assert.Equal("Admin", viewModel.CurrentRole);
        Assert.Equal("ແອັດມິນ", viewModel.CurrentRoleDisplay);
        Assert.False(string.IsNullOrWhiteSpace(viewModel.LoginTime));
        Assert.False(string.IsNullOrWhiteSpace(viewModel.CurrentDate));

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void CustomersCommand_CreatesCustomerPage()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer>());

        var customerViewModel = new CustomerViewModel(customerRepository.Object);
        var viewModel = CreateViewModel(CreateEmployee(), out _, out var navigationService);

        navigationService
            .Setup(x => x.CreateViewModel<CustomerViewModel>())
            .Returns(customerViewModel);

        viewModel.CustomersCommand.Execute(null);

        Assert.Same(customerViewModel, viewModel.CurrentPage);

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void SaleCommand_WithLoggedInEmployee_CreatesSalesPage()
    {
        var employee = CreateEmployee();
        var salesViewModel = CreateSalesViewModel(employee);
        var viewModel = CreateViewModel(employee, out _, out var navigationService);

        navigationService
            .Setup(x => x.CreateViewModelWithArgs<SalesViewModel>(It.IsAny<object[]>()))
            .Returns(salesViewModel);

        viewModel.SaleCommand.Execute(null);

        Assert.Same(salesViewModel, viewModel.CurrentPage);

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void SaleCommand_WithoutLoggedInEmployee_ShowsError()
    {
        var viewModel = CreateViewModel(null, out var dialogService, out _);

        viewModel.SaleCommand.Execute(null);

        dialogService.Verify(
            x => x.ShowErrorAsync(It.Is<string>(message => message.Contains("ບໍ່ມີຜູ້ໃຊ້"))),
            Times.Once);

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void ProfileCommand_WithLoggedInEmployee_CreatesProfilePage()
    {
        var employee = CreateEmployee();
        var profileViewModel = CreateProfileViewModel(employee);
        var viewModel = CreateViewModel(employee, out _, out var navigationService);

        navigationService
            .Setup(x => x.CreateViewModelWithArgs<ProfileViewModel>(It.IsAny<object[]>()))
            .Returns(profileViewModel);

        viewModel.ProfileCommand.Execute(null);

        Assert.Same(profileViewModel, viewModel.CurrentPage);

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void HomeCommand_ClearsCurrentPage()
    {
        var viewModel = CreateViewModel(CreateEmployee(), out _, out _);
        viewModel.CurrentPage = new ViewModelBase();

        viewModel.HomeCommand.Execute(null);

        Assert.Null(viewModel.CurrentPage);

        viewModel.LogoutCommand.Execute(null);
    }

    [AvaloniaFact]
    public void LogoutCommand_RaisesEvent()
    {
        var viewModel = CreateViewModel(CreateEmployee(), out _, out _);
        var logoutRaised = false;
        viewModel.LogoutRequested += (_, _) => logoutRaised = true;

        viewModel.LogoutCommand.Execute(null);

        Assert.True(logoutRaised);
    }

    private static MainWindowViewModel CreateViewModel(
        Employee? employee,
        out Mock<IDialogService> dialogService,
        out Mock<INavigationService> navigationService)
    {
        dialogService = new Mock<IDialogService>();
        dialogService.Setup(x => x.ShowErrorAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        dialogService.Setup(x => x.ShowSuccessAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        dialogService.Setup(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MsBox.Avalonia.Enums.Icon>())).Returns(Task.CompletedTask);
        dialogService.Setup(x => x.ShowConfirmationAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        navigationService = new Mock<INavigationService>();

        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(x => x.IsDarkTheme).Returns(false);

        return new MainWindowViewModel(employee, dialogService.Object, navigationService.Object, themeService.Object);
    }

    private static Employee CreateEmployee(string position = "Employee")
        => new()
        {
            Id = "EMP001",
            Name = "Jane",
            Surname = "Doe",
            Position = position,
            Username = "jane"
        };

    private static SalesViewModel CreateSalesViewModel(Employee employee)
    {
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

    private static ProfileViewModel CreateProfileViewModel(Employee employee)
    {
        var employeeRepository = new Mock<IEmployeeRepository>();
        employeeRepository.Setup(x => x.UpdateEmployeeProfileAsync(It.IsAny<Employee>())).ReturnsAsync(true);

        var credentialsRepository = new Mock<IEmployeeCredentialsRepository>();
        credentialsRepository.Setup(x => x.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        return new ProfileViewModel(employee, employeeRepository.Object, credentialsRepository.Object);
    }
}
