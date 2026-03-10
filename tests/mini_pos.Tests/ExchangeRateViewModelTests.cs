using Xunit;

using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mini_pos.Tests;

public class ExchangeRateViewModelTests
{
    [Fact]
    public void AddExchangeRate_InvalidInput_ValidationFails()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();
        var vm = new ExchangeRateViewModel(mockRepo.Object, null);

        vm.UsdRateInput = "invalid";
        vm.ThbRateInput = "alsoinvalid";

        vm.AddCommand.Execute(null);
    }

    [Fact]
    public void AddExchangeRate_ValidInput_CallsRepository()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();
        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(new List<ExchangeRate>());
        mockRepo.Setup(x => x.AddExchangeRateAsync(It.IsAny<ExchangeRate>())).ReturnsAsync(true);

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);

        vm.UsdRateInput = "23000";
        vm.ThbRateInput = "626";

        vm.AddCommand.Execute(null);

        mockRepo.Verify(x => x.AddExchangeRateAsync(It.Is<ExchangeRate>(e => e.UsdRate == 23000)), Times.Once);
    }

    [Fact]
    public void DeleteExchangeRate_CallsRepository()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();

        var existingRate = new ExchangeRate
        {
            Id = 1,
            UsdRate = 23000,
            ThbRate = 626,
            CreatedDate = DateTime.Now
        };

        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(new List<ExchangeRate> { existingRate });

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);
        vm.SelectedExchangeRate = vm.AllExchangeRates.FirstOrDefault();

        vm.DeleteCommand.Execute(null);

        Assert.Empty(vm.AllExchangeRates);
    }

    [Fact]
    public void Cancel_ResetsFields()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();
        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(new List<ExchangeRate>());

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);

        vm.UsdRateInput = "23000";
        vm.ThbRateInput = "626";

        vm.CancelCommand.Execute(null);

        Assert.Equal(string.Empty, vm.UsdRateInput);
        Assert.Equal(string.Empty, vm.ThbRateInput);
    }

    [Fact]
    public void FilterExchangeRates_OrdersByDateDescending()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();

        var rates = new List<ExchangeRate>
        {
            new ExchangeRate { Id = 1, UsdRate = 23000, ThbRate = 626, CreatedDate = DateTime.Now.AddDays(-1) },
            new ExchangeRate { Id = 2, UsdRate = 23100, ThbRate = 627, CreatedDate = DateTime.Now }
        };

        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(rates);

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);

        Assert.Equal(2, vm.ExchangeRates.Count);
        Assert.True(vm.ExchangeRates.First().CreatedDate > vm.ExchangeRates.Last().CreatedDate);
    }

    [Fact]
    public void OnSelectedExchangeRateChanged_CanDeleteIsTrue()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();
        var rate = new ExchangeRate { Id = 1, UsdRate = 23000, ThbRate = 626, CreatedDate = DateTime.Now };
        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(new List<ExchangeRate> { rate });

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);
        vm.SelectedExchangeRate = vm.AllExchangeRates.FirstOrDefault();

        Assert.True(vm.CanDelete);
    }

    [Fact]
    public void SelectedExchangeRateNull_CanDeleteIsFalse()
    {
        var mockRepo = new Mock<IExchangeRateRepository>();
        mockRepo.Setup(x => x.GetExchangeRateHistoryAsync()).ReturnsAsync(new List<ExchangeRate>());

        var vm = new ExchangeRateViewModel(mockRepo.Object, null);

        vm.SelectedExchangeRate = null;

        Assert.False(vm.CanDelete);
    }
}
