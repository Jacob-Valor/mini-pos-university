using System;
using System.Collections.Generic;

namespace mini_pos.Api.DTOs;

public record DailySalesReportDto(
    DateTime Date,
    decimal TotalSales,
    int TransactionCount,
    List<SaleDto> Sales
);

public record LowStockReportDto(
    string Barcode,
    string ProductName,
    int CurrentQuantity,
    int MinQuantity
);

public record SalesSummaryDto(
    decimal TotalRevenue,
    int TotalTransactions,
    decimal AverageTransaction
);
