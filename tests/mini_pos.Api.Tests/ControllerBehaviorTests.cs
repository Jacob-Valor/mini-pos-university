using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using mini_pos.Api.Controllers;
using mini_pos.Api.DTOs;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using Xunit;

namespace mini_pos.Api.Tests;

public class ControllerBehaviorTests
{
    [Fact]
    public async Task CreateProduct_MapsBrandAndCategoryNamesIntoResponse()
    {
        var productRepository = new Mock<IProductRepository>();
        Product? createdProduct = null;
        productRepository
            .Setup(x => x.AddProductAsync(It.IsAny<Product>()))
            .Callback<Product>(product => createdProduct = product)
            .ReturnsAsync(true);

        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.GetBrandsAsync()).ReturnsAsync(
            [new Brand { Id = "B001", Name = "Test Brand" }]);

        var productTypeRepository = new Mock<IProductTypeRepository>();
        productTypeRepository.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(
            [new ProductType { Id = "C001", Name = "Test Category" }]);

        var controller = new ProductsController(
            productRepository.Object,
            brandRepository.Object,
            productTypeRepository.Object);

        var response = await controller.CreateProduct(new CreateProductDto(
            "1234567890123",
            "Rice",
            "bag",
            10,
            2,
            100m,
            120m,
            "B001",
            "C001",
            "ມີ"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<ProductDto>(created.Value);

        Assert.NotNull(createdProduct);
        Assert.Equal("Test Brand", createdProduct!.BrandName);
        Assert.Equal("Test Category", createdProduct.CategoryName);
        Assert.Equal("Test Brand", payload.BrandName);
        Assert.Equal("Test Category", payload.CategoryName);
    }

    [Fact]
    public async Task UpdateProduct_WhenRepositoryReturnsFalse_ReturnsNotFound()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(false);

        var brandRepository = new Mock<IBrandRepository>();
        brandRepository.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());

        var productTypeRepository = new Mock<IProductTypeRepository>();
        productTypeRepository.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());

        var controller = new ProductsController(
            productRepository.Object,
            brandRepository.Object,
            productTypeRepository.Object);

        var response = await controller.UpdateProduct("missing", new UpdateProductDto(
            "Rice",
            "bag",
            10,
            2,
            100m,
            120m,
            "B001",
            "C001",
            "ມີ"));

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateCustomer_GeneratesCustomerIdAndReturnsCreatedPayload()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        Customer? createdCustomer = null;
        customerRepository
            .Setup(x => x.AddCustomerAsync(It.IsAny<Customer>()))
            .Callback<Customer>(customer => createdCustomer = customer)
            .ReturnsAsync(true);

        var controller = new CustomersController(customerRepository.Object);

        var response = await controller.CreateCustomer(new CreateCustomerDto(
            "Jane",
            "Doe",
            "ຍິງ",
            "Vientiane",
            "02012345678"));

        var created = Assert.IsType<CreatedAtActionResult>(response.Result);
        var payload = Assert.IsType<CustomerDto>(created.Value);

        Assert.NotNull(createdCustomer);
        Assert.StartsWith("CUS", createdCustomer!.Id);
        Assert.Equal(createdCustomer.Id, payload.CusId);
        Assert.Equal("Jane", payload.CusName);
    }

    [Fact]
    public async Task GetCustomer_WhenMissing_ReturnsNotFound()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(x => x.GetCustomersAsync()).ReturnsAsync(new List<Customer>());

        var controller = new CustomersController(customerRepository.Object);

        var response = await controller.GetCustomer("CUS0000001");

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetLatestExchangeRate_WhenMissing_ReturnsNotFound()
    {
        var exchangeRateRepository = new Mock<IExchangeRateRepository>();
        exchangeRateRepository.Setup(x => x.GetLatestExchangeRateAsync()).ReturnsAsync((ExchangeRate?)null);

        var controller = new ExchangeRatesController(exchangeRateRepository.Object);

        var response = await controller.GetLatest();

        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task GetLowStock_ReturnsOnlyProductsAtOrBelowMinimum()
    {
        var productRepository = new Mock<IProductRepository>();
        productRepository.Setup(x => x.GetProductsAsync()).ReturnsAsync(
        [
            new Product { Barcode = "LOW1", ProductName = "Low Stock", Quantity = 2, QuantityMin = 5 },
            new Product { Barcode = "OK1", ProductName = "Healthy Stock", Quantity = 6, QuantityMin = 5 },
            new Product { Barcode = "EDGE1", ProductName = "At Minimum", Quantity = 5, QuantityMin = 5 }
        ]);

        var controller = new ReportsController(Mock.Of<ISalesRepository>(), productRepository.Object);

        var response = await controller.GetLowStock();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<LowStockReportDto>>(ok.Value);

        Assert.Collection(
            payload,
            item => Assert.Equal("LOW1", item.Barcode),
            item => Assert.Equal("EDGE1", item.Barcode));
    }
}
