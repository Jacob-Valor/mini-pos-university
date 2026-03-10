using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System.Collections.Generic;
using System.Linq;

namespace mini_pos.Tests;

public class ProductTypeViewModelTests
{
    [Fact]
    public void AddProductType_EmptyName_ValidationFails()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.ProductTypeName = "";

        Assert.False(vm.CanAdd);
    }

    [Fact]
    public void AddProductType_ValidName_CanAddIsTrue()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.ProductTypeName = "Electronics";

        Assert.True(vm.CanAdd);
    }

    [Fact]
    public void AddProductType_ValidType_CallsRepository()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());
        mockRepo.Setup(x => x.AddProductTypeAsync(It.IsAny<ProductType>())).ReturnsAsync(true);

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.ProductTypeName = "Electronics";

        vm.AddCommand.Execute(null);

        mockRepo.Verify(x => x.AddProductTypeAsync(It.Is<ProductType>(t => t.Name == "Electronics")), Times.Once);
    }

    [Fact]
    public void EditProductType_ValidType_CallsRepository()
    {
        var mockRepo = new Mock<IProductTypeRepository>();

        var existingType = new ProductType { Id = "C001", Name = "Old Type" };

        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType> { existingType });
        mockRepo.Setup(x => x.UpdateProductTypeAsync(It.IsAny<ProductType>())).ReturnsAsync(true);

        var vm = new ProductTypeViewModel(mockRepo.Object, null);
        vm.SelectedProductType = vm.AllProductTypes.FirstOrDefault();

        vm.ProductTypeName = "New Type";
        vm.EditCommand.Execute(null);

        mockRepo.Verify(x => x.UpdateProductTypeAsync(It.Is<ProductType>(t => t.Name == "New Type")), Times.Once);
    }

    [Fact]
    public void DeleteProductType_CallsRepository()
    {
        var mockRepo = new Mock<IProductTypeRepository>();

        var existingType = new ProductType { Id = "C001", Name = "Electronics" };

        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType> { existingType });
        mockRepo.Setup(x => x.DeleteProductTypeAsync("C001")).ReturnsAsync(true);

        var vm = new ProductTypeViewModel(mockRepo.Object, null);
        vm.SelectedProductType = vm.AllProductTypes.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockRepo.Verify(x => x.DeleteProductTypeAsync("C001"), Times.Once);
    }

    [Fact]
    public void Cancel_ResetsFields()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.ProductTypeName = "Electronics";
        vm.SelectedProductType = new ProductType { Id = "C001", Name = "Electronics" };

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.ProductTypeName);
        Assert.Null(vm.SelectedProductType);
    }

    [Fact]
    public void FilterProductTypes_ByName_FiltersCorrectly()
    {
        var mockRepo = new Mock<IProductTypeRepository>();

        var types = new List<ProductType>
        {
            new ProductType { Id = "C001", Name = "Electronics" },
            new ProductType { Id = "C002", Name = "Clothing" }
        };

        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.SearchText = "Elec";

        Assert.Single(vm.ProductTypes);
        Assert.Equal("Electronics", vm.ProductTypes.First().Name);
    }

    [Fact]
    public void FilterProductTypes_EmptySearch_ReturnsAll()
    {
        var mockRepo = new Mock<IProductTypeRepository>();

        var types = new List<ProductType>
        {
            new ProductType { Id = "C001", Name = "Electronics" },
            new ProductType { Id = "C002", Name = "Clothing" }
        };

        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(types);

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.SearchText = "";

        Assert.Equal(2, vm.ProductTypes.Count);
    }

    [Fact]
    public void OnSelectedProductTypeChanged_PopulatesFields()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        var type = new ProductType { Id = "C001", Name = "Electronics" };
        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType> { type });

        var vm = new ProductTypeViewModel(mockRepo.Object, null);
        vm.SelectedProductType = vm.AllProductTypes.FirstOrDefault();

        Assert.Equal("Electronics", vm.ProductTypeName);
        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedProductTypeNull_CanEditOrDeleteIsFalse()
    {
        var mockRepo = new Mock<IProductTypeRepository>();
        mockRepo.Setup(x => x.GetProductTypesAsync()).ReturnsAsync(new List<ProductType>());

        var vm = new ProductTypeViewModel(mockRepo.Object, null);

        vm.SelectedProductType = null;

        Assert.False(vm.CanEditOrDelete);
    }
}
