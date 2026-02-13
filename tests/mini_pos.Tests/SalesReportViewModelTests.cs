using Xunit;
using mini_pos.ViewModels;
using mini_pos.Models;
using mini_pos.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace mini_pos.Tests;

public class SalesReportViewModelTests
{
    [Fact]
    public void Search_StartDateAfterEndDate_ShowsError()
    {
        var mockRepo = new Mock<ISalesRepository>();
        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.StartDate = DateTimeOffset.Now.AddDays(1);
        vm.EndDate = DateTimeOffset.Now;

        vm.SearchCommand.Execute(null);
    }

    [Fact]
    public void Search_ValidDates_CalculatesTotal()
    {
        var mockRepo = new Mock<ISalesRepository>();

        var items = new List<SalesReportItem>
        {
            new SalesReportItem { Total = 100000 },
            new SalesReportItem { Total = 200000 }
        };

        mockRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(items);

        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.SearchCommand.Execute(null);

        Assert.Equal(300000, vm.TotalAmount);
    }

    [Fact]
    public void Search_ValidDates_PopulatesReportItems()
    {
        var mockRepo = new Mock<ISalesRepository>();

        var items = new List<SalesReportItem>
        {
            new SalesReportItem { Total = 100000 },
            new SalesReportItem { Total = 200000 }
        };

        mockRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(items);

        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.SearchCommand.Execute(null);

        Assert.Equal(2, vm.ReportItems.Count);
    }

    [Fact]
    public void Search_EmptyResults_SetsZeroTotal()
    {
        var mockRepo = new Mock<ISalesRepository>();
        mockRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<SalesReportItem>());

        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.SearchCommand.Execute(null);

        Assert.Equal(0, vm.TotalAmount);
    }

    [Fact]
    public void Reset_ClearsAllFields()
    {
        var mockRepo = new Mock<ISalesRepository>();

        var items = new List<SalesReportItem>
        {
            new SalesReportItem { Total = 100000 }
        };

        mockRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(items);

        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.StartDate = DateTimeOffset.Now.AddDays(-7);
        vm.EndDate = DateTimeOffset.Now;
        vm.TotalAmount = 100000;

        vm.ResetCommand.Execute(null);

        Assert.Equal(DateTimeOffset.Now.Date, vm.StartDate.Date);
        Assert.Equal(DateTimeOffset.Now.Date, vm.EndDate.Date);
        Assert.Equal(0, vm.TotalAmount);
        Assert.Empty(vm.ReportItems);
    }

    [Fact]
    public void Print_EmptyReport_ShowsError()
    {
        var mockRepo = new Mock<ISalesRepository>();
        var mockReportService = new Mock<IReportService>();

        var vm = new SalesReportViewModel(mockRepo.Object, mockReportService.Object, null);

        vm.PrintCommand.Execute(null);
    }

    [Fact]
    public void Print_WithReportItems_CallsReportService()
    {
        var mockRepo = new Mock<ISalesRepository>();
        var mockReportService = new Mock<IReportService>();

        var items = new List<SalesReportItem>
        {
            new SalesReportItem { Total = 100000 }
        };

        mockRepo.Setup(x => x.GetSalesReportAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(items);

        var vm = new SalesReportViewModel(mockRepo.Object, mockReportService.Object, null);
        vm.SearchCommand.Execute(null);

        vm.PrintCommand.Execute(null);

        mockReportService.Verify(x => x.GenerateSalesReport(
            It.IsAny<List<SalesReportItem>>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Print_NoReportService_ShowsError()
    {
        var mockRepo = new Mock<ISalesRepository>();

        var vm = new SalesReportViewModel(mockRepo.Object, null, null);

        vm.ReportItems.Add(new SalesReportItem { Total = 100000 });

        vm.PrintCommand.Execute(null);
    }
}
