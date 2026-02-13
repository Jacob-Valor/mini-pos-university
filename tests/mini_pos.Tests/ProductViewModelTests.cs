using Xunit;
using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;
using Moq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mini_pos.Tests;

public class ProductViewModelTests
{
    [Fact]
    public void AddProduct_EmptyName_ShowsValidationError()
    {
        var vm = CreateProductViewModel();

        vm.ProductId = "P001";
        vm.ProductName = "";
        vm.ProductUnit = "pcs";
        vm.SelectedBrandItem = vm.Brands.FirstOrDefault();
        vm.SelectedTypeItem = vm.ProductTypes.FirstOrDefault();
        vm.SelectedStatusItem = "ມີ";

        vm.AddCommand.Execute(null);

        Assert.Equal("ກະລຸນາປ້ອນລະຫັດ ແລະ ຊື່ສິນຄ້າ", vm.ErrorMessage);
        Assert.True(vm.HasError);
    }

    [Fact]
    public void AddProduct_MissingBrandOrType_ShowsValidationError()
    {
        var vm = CreateProductViewModel();

        vm.ProductId = "P001";
        vm.ProductName = "Product 1";
        vm.ProductUnit = "pcs";
        vm.SelectedStatusItem = "ມີ";

        vm.AddCommand.Execute(null);

        Assert.Equal("ກະລຸນາເລືອກຍີ່ຫໍ້, ປະເພດ ແລະ ສະຖານະ", vm.ErrorMessage);
        Assert.True(vm.HasError);
    }

    [Fact]
    public void AddProduct_ValidProduct_CallsRepository()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        var brands = new List<Brand> { new Brand { Id = "B001", Name = "Brand1" } };
        var types = new List<ProductType> { new ProductType { Id = "T001", Name = "Type1" } };

        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);
        mockProductRepo.Setup(x => x.ProductExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        mockProductRepo.Setup(x => x.AddProductAsync(It.IsAny<Product>())).ReturnsAsync(true);

        var vm = new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);

        vm.ProductId = "P001";
        vm.ProductName = "Product 1";
        vm.ProductUnit = "pcs";
        vm.SelectedBrandItem = vm.Brands.FirstOrDefault();
        vm.SelectedTypeItem = vm.ProductTypes.FirstOrDefault();
        vm.SelectedStatusItem = "ມີ";

        vm.AddCommand.Execute(null);

        mockProductRepo.Verify(x => x.AddProductAsync(It.Is<Product>(p => p.Barcode == "P001")), Times.Once);
    }

    [Fact]
    public void AddProduct_DuplicateId_ShowsError()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        var brands = new List<Brand> { new Brand { Id = "B001", Name = "Brand1" } };
        var types = new List<ProductType> { new ProductType { Id = "T001", Name = "Type1" } };

        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);
        mockProductRepo.Setup(x => x.ProductExistsAsync("P001")).ReturnsAsync(true);

        var vm = new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);

        vm.ProductId = "P001";
        vm.ProductName = "Product 1";
        vm.SelectedBrandItem = vm.Brands.FirstOrDefault();
        vm.SelectedTypeItem = vm.ProductTypes.FirstOrDefault();
        vm.SelectedStatusItem = "ມີ";

        vm.AddCommand.Execute(null);

        Assert.Contains("P001", vm.ErrorMessage);
        Assert.Contains("ມີຢູ່", vm.ErrorMessage);
    }

    [Fact]
    public void EditProduct_ValidProduct_CallsRepository()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        var existingProduct = new Product
        {
            Barcode = "P001",
            ProductName = "Old Name",
            Unit = "pcs",
            Quantity = 10,
            QuantityMin = 5,
            CostPrice = 100,
            RetailPrice = 150,
            BrandId = "B001",
            BrandName = "Brand1",
            CategoryId = "T001",
            CategoryName = "Type1",
            Status = "ມີ"
        };

        var brands = new List<Brand> { new Brand { Id = "B001", Name = "Brand1" } };
        var types = new List<ProductType> { new ProductType { Id = "T001", Name = "Type1" } };

        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);
        mockProductRepo.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<Product> { existingProduct });
        mockProductRepo.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(true);

        var vm = new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);
        vm.SelectedProduct = vm.AllProducts.FirstOrDefault();

        vm.ProductName = "New Name";
        vm.EditCommand.Execute(null);

        mockProductRepo.Verify(x => x.UpdateProductAsync(It.Is<Product>(p => p.ProductName == "New Name")), Times.Once);
    }

    [Fact]
    public void DeleteProduct_CallsRepository()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        var existingProduct = new Product
        {
            Barcode = "P001",
            ProductName = "Test Product",
            Unit = "pcs",
            Status = "ມີ"
        };

        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());
        mockProductRepo.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<Product> { existingProduct });
        mockProductRepo.Setup(x => x.DeleteProductAsync("P001")).ReturnsAsync(true);

        var vm = new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);
        vm.SelectedProduct = vm.AllProducts.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockProductRepo.Verify(x => x.DeleteProductAsync("P001"), Times.Once);
    }

    [Fact]
    public void Cancel_ResetsAllFields()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());
        mockProductRepo.Setup(x => x.GetProductsAsync()).ReturnsAsync(new List<Product>());

        var vm = new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);

        vm.ProductId = "P001";
        vm.ProductName = "Product 1";
        vm.ProductUnit = "pcs";
        vm.ProductQuantity = 100;

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.ProductId);
        Assert.Equal(string.Empty, vm.ProductName);
        Assert.Equal(string.Empty, vm.ProductUnit);
        Assert.Equal(0, vm.ProductQuantity);
        Assert.Null(vm.SelectedProduct);
    }

    [Fact]
    public void FilterProducts_ByName_FiltersCorrectly()
    {
        var vm = CreateProductViewModel();

        vm.SearchText = "Test";

        Assert.Single(vm.Products);
        Assert.Equal("Test Product", vm.Products.First().ProductName);
    }

    [Fact]
    public void FilterProducts_ByBarcode_FiltersCorrectly()
    {
        var vm = CreateProductViewModel();

        vm.SearchText = "002";

        Assert.Single(vm.Products);
        Assert.Equal("P002", vm.Products.First().Barcode);
    }

    [Fact]
    public void FilterProducts_EmptySearch_ReturnsAll()
    {
        var vm = CreateProductViewModel();

        vm.SearchText = "";

        Assert.Equal(2, vm.Products.Count);
    }

    [Fact]
    public void OnSelectedProductChanged_PopulatesFields()
    {
        var vm = CreateProductViewModel();

        vm.SelectedProduct = vm.AllProducts.First();

        Assert.Equal("P001", vm.ProductId);
        Assert.Equal("Test Product", vm.ProductName);
        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedProductNull_CanEditOrDeleteIsFalse()
    {
        var vm = CreateProductViewModel();

        vm.SelectedProduct = null;

        Assert.False(vm.CanEditOrDelete);
    }

    private static ProductViewModel CreateProductViewModel()
    {
        var mockProductRepo = new Mock<IProductRepository>();
        var mockBrandRepo = new Mock<IBrandRepository>();
        var mockTypeRepo = new Mock<IProductTypeRepository>();

        var products = new List<Product>
        {
            new Product { Barcode = "P001", ProductName = "Test Product", Unit = "pcs", Status = "ມີ" },
            new Product { Barcode = "P002", ProductName = "Another Product", Unit = "box", Status = "ໝົດ" }
        };

        var brands = new List<Brand> { new Brand { Id = "B001", Name = "Brand1" } };
        var types = new List<ProductType> { new ProductType { Id = "T001", Name = "Type1" } };

        mockProductRepo.Setup(x => x.GetProductsAsync()).ReturnsAsync(products);
        mockBrandRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);
        mockTypeRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);

        return new ProductViewModel(mockProductRepo.Object, mockBrandRepo.Object, mockTypeRepo.Object, null);
    }
}
