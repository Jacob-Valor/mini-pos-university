using Xunit;
using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mini_pos.Tests;

public class CustomerViewModelTests
{
    [Fact]
    public void AddCustomer_EmptyName_ShowsValidationError()
    {
        var vm = CreateCustomerViewModel();

        vm.Name = "";

        vm.AddCommand.Execute(null);

        Assert.NotNull(vm);
    }

    [Fact]
    public void AddCustomer_ValidCustomer_CallsRepository()
    {
        var mockRepo = new Mock<ICustomerRepository>();
        mockRepo.Setup(x => x.AddCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(true);

        var vm = new CustomerViewModel(mockRepo.Object, null);

        vm.Name = "John";
        vm.Surname = "Doe";
        vm.Gender = "ຊາຍ";
        vm.PhoneNumber = "12345678";
        vm.Address = "Vientiane";

        vm.AddCommand.Execute(null);

        mockRepo.Verify(x => x.AddCustomerAsync(It.Is<Customer>(c => c.Name == "John")), Times.Once);
    }

    [Fact]
    public void EditCustomer_ValidCustomer_CallsRepository()
    {
        var mockRepo = new Mock<ICustomerRepository>();

        var existingCustomer = new Customer
        {
            Id = "CUS0000001",
            Name = "Old Name",
            Surname = "Surname",
            Gender = "ຊາຍ",
            PhoneNumber = "12345678",
            Address = "Address"
        };

        mockRepo.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer> { existingCustomer });
        mockRepo.Setup(x => x.UpdateCustomerAsync(It.IsAny<Customer>())).ReturnsAsync(true);

        var vm = new CustomerViewModel(mockRepo.Object, null);
        vm.SelectedCustomer = vm.AllCustomers.FirstOrDefault();

        vm.Name = "New Name";
        vm.EditCommand.Execute(null);

        mockRepo.Verify(x => x.UpdateCustomerAsync(It.Is<Customer>(c => c.Name == "New Name")), Times.Once);
    }

    [Fact]
    public void DeleteCustomer_CallsRepository()
    {
        var mockRepo = new Mock<ICustomerRepository>();

        var existingCustomer = new Customer
        {
            Id = "CUS0000001",
            Name = "John",
            Surname = "Doe"
        };

        mockRepo.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer> { existingCustomer });
        mockRepo.Setup(x => x.DeleteCustomerAsync("CUS0000001")).ReturnsAsync(true);

        var vm = new CustomerViewModel(mockRepo.Object, null);
        vm.SelectedCustomer = vm.AllCustomers.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockRepo.Verify(x => x.DeleteCustomerAsync("CUS0000001"), Times.Once);
    }

    [Fact]
    public void Cancel_ResetsAllFields()
    {
        var vm = CreateCustomerViewModel();

        vm.Name = "John";
        vm.Surname = "Doe";
        vm.PhoneNumber = "12345678";
        vm.Address = "Vientiane";
        vm.Gender = "ຍິງ";

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.Name);
        Assert.Equal(string.Empty, vm.Surname);
        Assert.Equal(string.Empty, vm.PhoneNumber);
        Assert.Equal(string.Empty, vm.Address);
        Assert.Equal("ຊາຍ", vm.Gender);
        Assert.Null(vm.SelectedCustomer);
    }

    [Fact]
    public void FilterCustomers_ByName_FiltersCorrectly()
    {
        var vm = CreateCustomerViewModel();

        vm.SearchText = "John";

        Assert.Single(vm.Customers);
        Assert.Equal("John", vm.Customers.First().Name);
    }

    [Fact]
    public void FilterCustomers_BySurname_FiltersCorrectly()
    {
        var vm = CreateCustomerViewModel();

        vm.SearchText = "Doe";

        Assert.Single(vm.Customers);
        Assert.Equal("Doe", vm.Customers.First().Surname);
    }

    [Fact]
    public void FilterCustomers_ByPhone_FiltersCorrectly()
    {
        var vm = CreateCustomerViewModel();

        vm.SearchText = "123";

        Assert.Single(vm.Customers);
        Assert.Equal("12345678", vm.Customers.First().PhoneNumber);
    }

    [Fact]
    public void FilterCustomers_EmptySearch_ReturnsAll()
    {
        var vm = CreateCustomerViewModel();

        vm.SearchText = "";

        Assert.Equal(2, vm.Customers.Count);
    }

    [Fact]
    public void OnSelectedCustomerChanged_PopulatesFields()
    {
        var vm = CreateCustomerViewModel();

        vm.SelectedCustomer = vm.AllCustomers.FirstOrDefault();

        Assert.Equal("CUS0000001", vm.CustomerId);
        Assert.Equal("John", vm.Name);
        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedCustomerNull_CanEditOrDeleteIsFalse()
    {
        var vm = CreateCustomerViewModel();

        vm.SelectedCustomer = null;

        Assert.False(vm.CanEditOrDelete);
    }

    [Fact]
    public void OnNameChanged_CanAddBecomesTrue()
    {
        var vm = CreateCustomerViewModel();

        vm.Name = "Test";

        Assert.True(vm.CanAdd);
    }

    [Fact]
    public void OnNameChanged_CanAddBecomesFalseWhenEmpty()
    {
        var vm = CreateCustomerViewModel();
        vm.Name = "Test";

        vm.Name = "";

        Assert.False(vm.CanAdd);
    }

    [Fact]
    public void GenerateNewId_FormatsCorrectly()
    {
        var vm = CreateCustomerViewModel();

        var id = vm.CustomerId;

        Assert.StartsWith("CUS", id);
    }

    private static CustomerViewModel CreateCustomerViewModel()
    {
        var mockRepo = new Mock<ICustomerRepository>();

        var customers = new List<Customer>
        {
            new Customer { Id = "CUS0000001", Name = "John", Surname = "Doe", Gender = "ຊາຍ", PhoneNumber = "12345678" },
            new Customer { Id = "CUS0000002", Name = "Jane", Surname = "Smith", Gender = "ຍິງ", PhoneNumber = "87654321" }
        };

        mockRepo.Setup(x => x.GetCustomersAsync()).ReturnsAsync(customers);

        return new CustomerViewModel(mockRepo.Object, null);
    }
}
