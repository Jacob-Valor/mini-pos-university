using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Avalonia.Headless.XUnit;

using mini_pos.Models;
using mini_pos.Services;

using Xunit;

namespace mini_pos.Tests;

public class ReportServiceTests
{
    [AvaloniaFact]
    public void GenerateSalesReport_WithValidItems_CreatesPdfFile()
    {
        var service = new ReportService();
        var tempDirectory = CreateTempDirectory();
        var outputPath = Path.Combine(tempDirectory, "sales-report.pdf");

        try
        {
            service.GenerateSalesReport(CreateItems(), new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), 350m, outputPath);

            Assert.True(File.Exists(outputPath));
            Assert.True(new FileInfo(outputPath).Length > 0);

            using var stream = File.OpenRead(outputPath);
            Span<byte> header = stackalloc byte[4];
            var bytesRead = stream.Read(header);

            Assert.Equal(4, bytesRead);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(header));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
                Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [AvaloniaFact]
    public void GenerateSalesReport_WhenOutputDirectoryDoesNotExist_CreatesDirectoryAndPdf()
    {
        var service = new ReportService();
        var rootDirectory = CreateTempDirectory();
        var outputPath = Path.Combine(rootDirectory, "nested", "reports", "sales-report.pdf");

        try
        {
            service.GenerateSalesReport(CreateItems(), new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), 350m, outputPath);

            Assert.True(Directory.Exists(Path.GetDirectoryName(outputPath)!));
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
                Directory.Delete(rootDirectory, recursive: true);
        }
    }

    [Fact]
    public void GenerateSalesReport_WithBlankFilePath_ThrowsArgumentException()
    {
        var service = new ReportService();

        Assert.Throws<ArgumentException>(() =>
            service.GenerateSalesReport(CreateItems(), DateTime.Today, DateTime.Today, 350m, string.Empty));
    }

    private static List<SalesReportItem> CreateItems() =>
    [
        new SalesReportItem
        {
            No = 1,
            Barcode = "1234567890123",
            ProductName = "Rice",
            Unit = "bag",
            Quantity = 2,
            Price = 100m,
            Total = 200m
        },
        new SalesReportItem
        {
            No = 2,
            Barcode = "9876543210987",
            ProductName = "Sugar",
            Unit = "bag",
            Quantity = 1,
            Price = 150m,
            Total = 150m
        }
    ];

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mini-pos-report-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
