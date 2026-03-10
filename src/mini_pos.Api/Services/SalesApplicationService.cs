using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using mini_pos.Api.DTOs;
using mini_pos.Models;
using mini_pos.Services;

using Serilog;

namespace mini_pos.Api.Services;

public sealed class SalesApplicationService : ISalesApplicationService
{
    private readonly ISalesRepository _salesRepository;

    public SalesApplicationService(ISalesRepository salesRepository)
    {
        _salesRepository = salesRepository;
    }

    public async Task<SalesReportUseCaseResult> GetSalesReportAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var start = (startDate ?? DateTime.Today).Date;
        var end = (endDate ?? DateTime.Today).Date;

        if (end < start)
            return new(false, [], "endDate must be greater than or equal to startDate");

        try
        {
            var report = await _salesRepository.GetSalesReportAsync(start, end);
            return new(true, report);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching sales report");
            return new(false, [], "Failed to fetch sales report");
        }
    }

    public async Task<CreateSaleUseCaseResult> CreateSaleAsync(
        CreateSaleDto? dto,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCreateSaleRequest(dto);
        if (validationError is not null)
            return new(false, validationError);

        var sale = new Sale
        {
            ExchangeRateId = dto!.ExId,
            CustomerId = dto.CusId,
            EmployeeId = dto.EmpId,
            DateSale = DateTime.UtcNow,
            SubTotal = dto.Subtotal,
            Pay = dto.Pay,
            Change = dto.MoneyChange
        };

        var details = dto.Items.Select(item => new SaleDetail
        {
            ProductId = item.ProductId,
            Quantity = item.Qty,
            Price = item.Price,
            Total = item.Total
        }).ToList();

        try
        {
            var success = await _salesRepository.CreateSaleAsync(sale, details);
            return success ? new(true) : new(false, "Failed to create sale");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating sale");
            return new(false, "Failed to create sale");
        }
    }

    private static string? ValidateCreateSaleRequest(CreateSaleDto? dto)
    {
        if (dto is null)
            return "Request body is required";

        if (string.IsNullOrWhiteSpace(dto.CusId))
            return "Customer ID is required";

        if (string.IsNullOrWhiteSpace(dto.EmpId))
            return "Employee ID is required";

        if (dto.Subtotal < 0 || dto.Pay < 0 || dto.MoneyChange < 0)
            return "Payment values must be non-negative";

        if (dto.Pay < dto.Subtotal)
            return "Pay amount must be greater than or equal to subtotal";

        if (dto.Items is null || dto.Items.Count == 0)
            return "Sale must include at least one item";

        if (dto.Items.Any(item =>
                string.IsNullOrWhiteSpace(item.ProductId) ||
                item.Qty <= 0 ||
                item.Price < 0 ||
                item.Total < 0))
        {
            return "Sale items must have product id, positive quantity, and non-negative price/total";
        }

        return null;
    }
}
