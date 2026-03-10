using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System.Collections.Generic;
using System.Linq;

namespace mini_pos.Tests;

public class BrandViewModelTests
{
    [Fact]
    public void AddBrand_EmptyName_ValidationFails()
    {
        var mockRepo = new Mock<IBrandRepository>();
        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.BrandName = "";

        Assert.False(vm.CanAdd);
    }

    [Fact]
    public void AddBrand_ValidName_CanAddIsTrue()
    {
        var mockRepo = new Mock<IBrandRepository>();
        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.BrandName = "Nike";

        Assert.True(vm.CanAdd);
    }

    [Fact]
    public void AddBrand_ValidBrand_CallsRepository()
    {
        var mockRepo = new Mock<IBrandRepository>();
        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());
        mockRepo.Setup(x => x.AddBrandAsync(It.IsAny<Brand>())).ReturnsAsync(true);

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.BrandName = "Nike";

        vm.AddCommand.Execute(null);

        mockRepo.Verify(x => x.AddBrandAsync(It.Is<Brand>(b => b.Name == "Nike")), Times.Once);
    }

    [Fact]
    public void EditBrand_ValidBrand_CallsRepository()
    {
        var mockRepo = new Mock<IBrandRepository>();

        var existingBrand = new Brand { Id = "B001", Name = "Old Brand" };

        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand> { existingBrand });
        mockRepo.Setup(x => x.UpdateBrandAsync(It.IsAny<Brand>())).ReturnsAsync(true);

        var vm = new BrandViewModel(mockRepo.Object, null);
        vm.SelectedBrand = vm.AllBrands.FirstOrDefault();

        vm.BrandName = "New Brand";
        vm.EditCommand.Execute(null);

        mockRepo.Verify(x => x.UpdateBrandAsync(It.Is<Brand>(b => b.Name == "New Brand")), Times.Once);
    }

    [Fact]
    public void DeleteBrand_CallsRepository()
    {
        var mockRepo = new Mock<IBrandRepository>();

        var existingBrand = new Brand { Id = "B001", Name = "Nike" };

        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand> { existingBrand });
        mockRepo.Setup(x => x.DeleteBrandAsync("B001")).ReturnsAsync(true);

        var vm = new BrandViewModel(mockRepo.Object, null);
        vm.SelectedBrand = vm.AllBrands.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        mockRepo.Verify(x => x.DeleteBrandAsync("B001"), Times.Once);
    }

    [Fact]
    public void Cancel_ResetsFields()
    {
        var mockRepo = new Mock<IBrandRepository>();
        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.BrandName = "Nike";
        vm.SelectedBrand = new Brand { Id = "B001", Name = "Nike" };

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.BrandName);
        Assert.Null(vm.SelectedBrand);
    }

    [Fact]
    public void FilterBrands_ByName_FiltersCorrectly()
    {
        var mockRepo = new Mock<IBrandRepository>();

        var brands = new List<Brand>
        {
            new Brand { Id = "B001", Name = "Nike" },
            new Brand { Id = "B002", Name = "Adidas" }
        };

        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.SearchText = "Nik";

        Assert.Single(vm.Brands);
        Assert.Equal("Nike", vm.Brands.First().Name);
    }

    [Fact]
    public void FilterBrands_EmptySearch_ReturnsAll()
    {
        var mockRepo = new Mock<IBrandRepository>();

        var brands = new List<Brand>
        {
            new Brand { Id = "B001", Name = "Nike" },
            new Brand { Id = "B002", Name = "Adidas" }
        };

        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(brands);

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.SearchText = "";

        Assert.Equal(2, vm.Brands.Count);
    }

    [Fact]
    public void OnSelectedBrandChanged_PopulatesFields()
    {
        var mockRepo = new Mock<IBrandRepository>();
        var brand = new Brand { Id = "B001", Name = "Nike" };
        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand> { brand });

        var vm = new BrandViewModel(mockRepo.Object, null);
        vm.SelectedBrand = vm.AllBrands.FirstOrDefault();

        Assert.Equal("Nike", vm.BrandName);
        Assert.True(vm.CanEditOrDelete);
    }

    [Fact]
    public void SelectedBrandNull_CanEditOrDeleteIsFalse()
    {
        var mockRepo = new Mock<IBrandRepository>();
        mockRepo.Setup(x => x.GetBrandsAsync()).ReturnsAsync(new List<Brand>());

        var vm = new BrandViewModel(mockRepo.Object, null);

        vm.SelectedBrand = null;

        Assert.False(vm.CanEditOrDelete);
    }
}
