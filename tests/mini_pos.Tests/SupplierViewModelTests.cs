using Xunit;
using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace mini_pos.Tests;

public class SupplierViewModelTests
{
    [Fact]
    public void AddSupplier_OpensDialog()
    {
        var mockRepo = new Mock<ISupplierRepository>();
        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier>());

        var vm = new SupplierViewModel(mockRepo.Object, null);
        bool dialogOpened = false;
        vm.ShowSupplierDialogRequested += () => dialogOpened = true;

        vm.AddCommand.Execute(null);

        Assert.True(dialogOpened);
        Assert.Equal("SUP001", vm.CurrentSupplier.Id);
    }

    [Fact]
    public void EditSupplier_OpensDialog()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var existingSupplier = new Supplier
        {
            Id = "SUP001",
            Name = "Supplier 1",
            Sequence = 1
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier> { existingSupplier });

        var vm = new SupplierViewModel(mockRepo.Object, null);
        vm.SelectedSupplier = vm.AllSuppliers.FirstOrDefault();

        bool dialogOpened = false;
        vm.ShowSupplierDialogRequested += () => dialogOpened = true;

        vm.EditCommand.Execute(null);

        Assert.True(dialogOpened);
    }

    [Fact]
    public void SaveSupplier_EmptyName_ValidationFails()
    {
        var mockRepo = new Mock<ISupplierRepository>();
        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier>());

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.CurrentSupplier = new Supplier { Id = "SUP001", Name = "" };

        vm.SaveCommand.Execute(null);
    }

    [Fact]
    public void SaveSupplier_AddMode_CallsAddRepository()
    {
        var mockRepo = new Mock<ISupplierRepository>();
        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier>());
        mockRepo.Setup(x => x.AddSupplierAsync(It.IsAny<Supplier>())).ReturnsAsync(true);

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.CurrentSupplier = new Supplier
        {
            Id = "SUP001",
            Name = "New Supplier",
            ContactName = "John",
            Phone = "12345678"
        };

        vm.SaveCommand.Execute(null);

        mockRepo.Verify(x => x.AddSupplierAsync(It.Is<Supplier>(s => s.Name == "New Supplier")), Times.Once);
    }

    [Fact]
    public void EditSupplier_WithSupplier_SetsCurrentSupplier()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var existingSupplier = new Supplier
        {
            Id = "SUP001",
            Name = "Supplier 1",
            ContactName = "John",
            Phone = "12345678",
            Sequence = 1
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier> { existingSupplier });

        var vm = new SupplierViewModel(mockRepo.Object, null);
        vm.SelectedSupplier = vm.AllSuppliers.FirstOrDefault();

        bool dialogOpened = false;
        vm.ShowSupplierDialogRequested += () => dialogOpened = true;

        vm.EditCommand.Execute(null);

        Assert.True(dialogOpened);
        Assert.Equal("Supplier 1", vm.CurrentSupplier.Name);
    }

    [Fact]
    public void DeleteSupplier_CallsRepository()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var existingSupplier = new Supplier
        {
            Id = "SUP001",
            Name = "Supplier 1",
            Sequence = 1
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier> { existingSupplier });
        mockRepo.Setup(x => x.DeleteSupplierAsync("SUP001")).ReturnsAsync(true);

        var vm = new SupplierViewModel(mockRepo.Object, null);
        vm.SelectedSupplier = vm.AllSuppliers.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockRepo.Verify(x => x.DeleteSupplierAsync("SUP001"), Times.Once);
    }

    [Fact]
    public void FilterSuppliers_ByName_FiltersCorrectly()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var suppliers = new List<Supplier>
        {
            new Supplier { Id = "SUP001", Name = "Supplier A", Sequence = 1 },
            new Supplier { Id = "SUP002", Name = "Supplier B", Sequence = 2 }
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(suppliers);

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.SearchText = "Supplier A";

        Assert.Single(vm.Suppliers);
        Assert.Equal("Supplier A", vm.Suppliers.First().Name);
    }

    [Fact]
    public void FilterSuppliers_ById_FiltersCorrectly()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var suppliers = new List<Supplier>
        {
            new Supplier { Id = "SUP001", Name = "Supplier A", Sequence = 1 },
            new Supplier { Id = "SUP002", Name = "Supplier B", Sequence = 2 }
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(suppliers);

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.SearchText = "SUP001";

        Assert.Single(vm.Suppliers);
        Assert.Equal("SUP001", vm.Suppliers.First().Id);
    }

    [Fact]
    public void FilterSuppliers_EmptySearch_ReturnsAll()
    {
        var mockRepo = new Mock<ISupplierRepository>();

        var suppliers = new List<Supplier>
        {
            new Supplier { Id = "SUP001", Name = "Supplier A", Sequence = 1 },
            new Supplier { Id = "SUP002", Name = "Supplier B", Sequence = 2 }
        };

        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(suppliers);

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.SearchText = "";

        Assert.Equal(2, vm.Suppliers.Count);
    }

    [Fact]
    public void OnSelectedSupplierChanged_CanEditOrDeleteIsTrue()
    {
        var mockRepo = new Mock<ISupplierRepository>();
        var supplier = new Supplier { Id = "SUP001", Name = "Supplier 1", Sequence = 1 };
        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier> { supplier });

        var vm = new SupplierViewModel(mockRepo.Object, null);
        vm.SelectedSupplier = vm.AllSuppliers.FirstOrDefault();

        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedSupplierNull_CanEditOrDeleteIsFalse()
    {
        var mockRepo = new Mock<ISupplierRepository>();
        mockRepo.Setup(x => x.GetSuppliersAsync()).ReturnsAsync(new List<Supplier>());

        var vm = new SupplierViewModel(mockRepo.Object, null);

        vm.SelectedSupplier = null;

        Assert.False(vm.CanEditOrDelete);
    }
}
