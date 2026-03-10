using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using mini_pos.Api.DTOs;
using mini_pos.Models;

namespace mini_pos.Api.Services;

public interface ISalesApplicationService
{
    Task<SalesReportUseCaseResult> GetSalesReportAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<CreateSaleUseCaseResult> CreateSaleAsync(
        CreateSaleDto? dto,
        CancellationToken cancellationToken = default);
}

public sealed record SalesReportUseCaseResult(
    bool IsSuccess,
    List<SalesReportItem> Items,
    string? ErrorMessage = null);

public sealed record CreateSaleUseCaseResult(
    bool IsSuccess,
    string? ErrorMessage = null);
