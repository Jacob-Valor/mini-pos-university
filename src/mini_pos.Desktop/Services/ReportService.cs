using System;
using System.Collections.Generic;
using System.IO;

using Avalonia;
using Avalonia.Platform;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

using mini_pos.Models;

using Serilog;

namespace mini_pos.Services;

public interface IReportService
{
    void GenerateSalesReport(List<SalesReportItem> items, DateTime startDate, DateTime endDate, decimal totalAmount, string filePath);
}

public class ReportService : IReportService
{
    static ReportService()
    {
        // License setup for QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;

        // Register Fonts
        try
        {
            // Load Regular Font
            var regularFontUri = new Uri("avares://mini_pos/Assets/Fonts/NotoSansLaoLooped-Regular.ttf");
            using (var assetStream = AssetLoader.Open(regularFontUri))
            using (var ms = new MemoryStream())
            {
                assetStream.CopyTo(ms);
                ms.Position = 0;
                FontManager.RegisterFont(ms);
            }

            // Load Bold Font
            var boldFontUri = new Uri("avares://mini_pos/Assets/Fonts/NotoSansLaoLooped-Bold.ttf");
            using (var assetStream = AssetLoader.Open(boldFontUri))
            using (var ms = new MemoryStream())
            {
                assetStream.CopyTo(ms);
                ms.Position = 0;
                FontManager.RegisterFont(ms);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering report fonts");
        }
    }

    public void GenerateSalesReport(List<SalesReportItem> items, DateTime startDate, DateTime endDate, decimal totalAmount, string filePath)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);


        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);

                var textStyle = TextStyle.Default.FontFamily("Noto Sans Lao Looped");

                page.Content()
                    .DefaultTextStyle(textStyle)
                    .Column(column =>
                    {
                        // Header
                        column.Item().AlignCenter().Text("ສາທາລະນະລັດ ປະຊາທິປະໄຕ ປະຊາຊົນລາວ").FontSize(12).FontFamily("Noto Sans Lao Looped");
                        column.Item().AlignCenter().Text("ສັນຕິພາບ ເອກະລາດ ປະຊາທິປະໄຕ ເອກະພາບ ວັດທະນະຖາວອນ").FontSize(12).FontFamily("Noto Sans Lao Looped");
                        column.Item().Height(10);
                        column.Item().AlignCenter().Text("ໃບລາຍງານການຂາຍສິນຄ້າ").FontSize(16).Bold().FontFamily("Noto Sans Lao Looped");
                        column.Item().Height(10);

                        // Date Range
                        column.Item().Text($"ເລີ່ມແຕ່ວັນທີ   {startDate:yyyy-MM-dd}   ຫາ   {endDate:yyyy-MM-dd}").FontSize(11).FontFamily("Noto Sans Lao Looped");
                        column.Item().Height(10);

                        // Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderTop(1).AlignCenter().Text("ລຳດັບ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                                header.Cell().BorderBottom(1).BorderTop(1).Text("ລາຍການສິນຄ້າ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                                header.Cell().BorderBottom(1).BorderTop(1).AlignCenter().Text("ຫົວໜ່ວຍ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                                header.Cell().BorderBottom(1).BorderTop(1).AlignCenter().Text("ຈໍານວນ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                                header.Cell().BorderBottom(1).BorderTop(1).AlignRight().Text("ລາຄາຕໍ່ໜ່ວຍ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                                header.Cell().BorderBottom(1).BorderTop(1).AlignRight().Text("ລາຄາລວມ").FontSize(10).Bold().FontFamily("Noto Sans Lao Looped");
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.No.ToString());
                                table.Cell().Element(CellStyle).Text(item.ProductName).FontFamily("Noto Sans Lao Looped");
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Unit).FontFamily("Noto Sans Lao Looped");
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Price.ToString("N0"));
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Total.ToString("N0"));
                            }

                            // Footer row for total
                            table.Cell().ColumnSpan(6).PaddingTop(10).AlignRight().Row(row =>
                            {
                                row.RelativeItem().Text("ລວມທັງໝົດ:").Bold().FontSize(12).FontFamily("Noto Sans Lao Looped");
                                row.ConstantItem(100).AlignRight().Text(totalAmount.ToString("N0")).Bold().FontSize(12);
                            });

                        });

                        column.Item().Height(20);

                        // Signatures
                        column.Item().AlignRight().Text($"ອັດຕະປື, ວັນທີ {DateTime.Now:dd/MM/yyyy}").FontSize(10).FontFamily("Noto Sans Lao Looped");
                        column.Item().AlignRight().Text("ຜູ້ຮັບຜິດຊອບ").FontSize(10).FontFamily("Noto Sans Lao Looped");
                    });
            });
        }).GeneratePdf(filePath);
    }

    // Helper to style table cells
    static IContainer CellStyle(IContainer container)
    {
        return container.PaddingVertical(5);
    }


}
