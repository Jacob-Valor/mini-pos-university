using mini_pos.Models;
using mini_pos.Validators;

using Xunit;

namespace mini_pos.Tests;

public class ValidatorTests
{
    [Fact]
    public void ProductValidator_WhenRetailPriceIsBelowCost_ReturnsValidationError()
    {
        var validator = new ProductValidator();
        var product = new Product
        {
            Barcode = "1234567890123",
            ProductName = "Rice",
            Unit = "bag",
            Quantity = 10,
            QuantityMin = 2,
            CostPrice = 100m,
            RetailPrice = 90m,
            BrandId = "B001",
            CategoryId = "C001",
            Status = "ມີ"
        };

        var result = validator.Validate(product);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Product.RetailPrice));
    }

    [Fact]
    public void ProductValidator_WithValidProduct_PassesValidation()
    {
        var validator = new ProductValidator();
        var product = new Product
        {
            Barcode = "1234567890123",
            ProductName = "Rice",
            Unit = "bag",
            Quantity = 10,
            QuantityMin = 2,
            CostPrice = 100m,
            RetailPrice = 120m,
            BrandId = "B001",
            CategoryId = "C001",
            Status = "ມີ"
        };

        var result = validator.Validate(product);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CustomerValidator_WhenPhoneNumberIsInvalid_ReturnsValidationError()
    {
        var validator = new CustomerValidator();
        var customer = new Customer
        {
            Id = "CUS0000001",
            Name = "Jane",
            Surname = "Doe",
            Gender = "ຍິງ",
            Address = "Vientiane",
            PhoneNumber = "12AB"
        };

        var result = validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(Customer.PhoneNumber));
    }

    [Fact]
    public void CustomerValidator_WhenPhoneNumberIsEmpty_PassesValidation()
    {
        var validator = new CustomerValidator();
        var customer = new Customer
        {
            Id = "CUS0000001",
            Name = "Jane",
            Surname = "Doe",
            Gender = "ຍິງ",
            Address = "Vientiane",
            PhoneNumber = string.Empty
        };

        var result = validator.Validate(customer);

        Assert.True(result.IsValid);
    }
}
