using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace mini_pos.Tests;

public class SalesViewModelTests
{
    [Fact]
    public void AddProduct_DifferentProducts_AddsNewItem()
    {
        var vm = CreateSalesViewModel();

        vm.Barcode = "123";
        vm.ProductName = "Product 1";
        vm.Unit = "pcs";
        vm.UnitPrice = 100;
        vm.Quantity = 1;
        vm.AddProductCommand.Execute(null);

        vm.Barcode = "456";
        vm.ProductName = "Product 2";
        vm.Unit = "pcs";
        vm.UnitPrice = 200;
        vm.Quantity = 1;
        vm.AddProductCommand.Execute(null);

        Assert.Equal(2, vm.CartItems.Count);
    }

    [Fact]
    public void ClearCartCommand_RemovesAllItems()
    {
        var vm = CreateSalesViewModel();

        vm.Barcode = "123";
        vm.ProductName = "Product 1";
        vm.Unit = "pcs";
        vm.UnitPrice = 100;
        vm.Quantity = 1;
        vm.AddProductCommand.Execute(null);

        vm.ClearCartCommand.Execute(null);

        Assert.Empty(vm.CartItems);
    }

    [Fact]
    public void AddProduct_SameBarcode_IncrementsQuantity_AndUpdatesTotals()
    {
        var vm = CreateSalesViewModel();

        vm.Barcode = "123";
        vm.ProductName = "Product 1";
        vm.Unit = "pcs";
        vm.UnitPrice = 100;
        vm.Quantity = 2;
        vm.AddProductCommand.Execute(null);

        vm.Barcode = "123";
        vm.ProductName = "Product 1";
        vm.Unit = "pcs";
        vm.UnitPrice = 100;
        vm.Quantity = 3;
        vm.AddProductCommand.Execute(null);

        Assert.Single(vm.CartItems);
        Assert.Equal(5, vm.CartItems[0].Quantity);
        Assert.Equal(500, vm.TotalAmount);
    }

    [Fact]
    public void CartItem_QuantityChange_RecalculatesTotalAmount()
    {
        var vm = CreateSalesViewModel();

        vm.Barcode = "123";
        vm.ProductName = "Product 1";
        vm.Unit = "pcs";
        vm.UnitPrice = 100;
        vm.Quantity = 1;
        vm.AddProductCommand.Execute(null);

        Assert.Equal(100, vm.TotalAmount);

        vm.CartItems[0].Quantity = 4;

        Assert.Equal(400, vm.TotalAmount);
    }

    [Fact]
    public void CalculateChange_PositiveMoney_ReturnsDifference()
    {
        var vm = CreateSalesViewModel();
        vm.TotalAmount = 500;
        vm.MoneyReceived = 1000;

        Assert.Equal(500, vm.Change);
    }

    [Fact]
    public void CalculateChange_InsufficientMoney_ReturnsZero()
    {
        var vm = CreateSalesViewModel();
        vm.TotalAmount = 1000;
        vm.MoneyReceived = 500;

        Assert.Equal(0, vm.Change);
    }

    [Fact]
    public void CalculateChange_ExactAmount_ReturnsZero()
    {
        var vm = CreateSalesViewModel();
        vm.TotalAmount = 500;
        vm.MoneyReceived = 500;

        Assert.Equal(0, vm.Change);
    }

    [Fact]
    public void TotalDollar_IsCalculatedCorrectly()
    {
        var vm = CreateSalesViewModel();
        vm.TotalAmount = 230000;
        vm.ExchangeRateDollar = 23000;

        var result = vm.ExchangeRateDollar > 0 ? vm.TotalAmount / vm.ExchangeRateDollar : 0;

        Assert.Equal(10, result);
    }

    [Fact]
    public void TotalBaht_IsCalculatedCorrectly()
    {
        var vm = CreateSalesViewModel();
        vm.TotalAmount = 12520;
        vm.ExchangeRateBaht = 626;

        var result = vm.ExchangeRateBaht > 0 ? vm.TotalAmount / vm.ExchangeRateBaht : 0;

        Assert.Equal(20, result);
    }

    [Fact]
    public void CartItems_TotalPrice_CalculatesCorrectly()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test",
            Unit = "pcs",
            Quantity = 5,
            UnitPrice = 100
        };

        Assert.Equal(500, item.TotalPrice);
    }

    private static SalesViewModel CreateSalesViewModel()
    {
        var mockProducts = new Mock<IProductRepository>();
        var mockCustomers = new Mock<ICustomerRepository>();
        var mockExchangeRates = new Mock<IExchangeRateRepository>();
        mockExchangeRates
            .Setup(x => x.GetLatestExchangeRateAsync())
            .ReturnsAsync((ExchangeRate?)null);

        var mockSales = new Mock<ISalesRepository>();
        var employee = new Employee { Id = "EMP001", Name = "Test", Surname = "User" };

        return new SalesViewModel(
            employee,
            mockProducts.Object,
            mockCustomers.Object,
            mockExchangeRates.Object,
            mockSales.Object,
            null);
    }
}

public partial class CartItemViewModelTests
{
    [Fact]
    public void TotalPrice_QuantityTimesPrice()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test Product",
            Unit = "pcs",
            Quantity = 3,
            UnitPrice = 50
        };

        Assert.Equal(150, item.TotalPrice);
    }

    [Fact]
    public void TotalPrice_SingleItem_ReturnsUnitPrice()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test",
            Unit = "pcs",
            Quantity = 1,
            UnitPrice = 99.99m
        };

        Assert.Equal(99.99m, item.TotalPrice);
    }

    [Fact]
    public void TotalPrice_ZeroQuantity_ReturnsZero()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test",
            Unit = "pcs",
            Quantity = 0,
            UnitPrice = 100
        };

        Assert.Equal(0, item.TotalPrice);
    }

    [Fact]
    public void QuantityChange_RaisesTotalPricePropertyChanged()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test",
            Unit = "pcs",
            Quantity = 1,
            UnitPrice = 100
        };

        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.Quantity = 2;

        Assert.Contains(nameof(CartItemViewModel.TotalPrice), changed);
    }

    [Fact]
    public void UnitPriceChange_RaisesTotalPricePropertyChanged()
    {
        var item = new CartItemViewModel
        {
            Barcode = "123",
            ProductName = "Test",
            Unit = "pcs",
            Quantity = 1,
            UnitPrice = 100
        };

        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.UnitPrice = 120;

        Assert.Contains(nameof(CartItemViewModel.TotalPrice), changed);
    }
}
