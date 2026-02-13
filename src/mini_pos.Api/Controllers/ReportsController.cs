using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using mini_pos.Api.DTOs;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISalesRepository _salesRepository;
    private readonly IProductRepository _productRepository;

    public ReportsController(ISalesRepository salesRepository, IProductRepository productRepository)
    {
        _salesRepository = salesRepository;
        _productRepository = productRepository;
    }

    [HttpGet("daily-sales")]
    public async Task<ActionResult<DailySalesReportDto>> GetDailySales([FromQuery] DateTime? date = null)
    {
        var targetDate = (date ?? DateTime.Today).Date;
        var startDate = targetDate;
        var endDate = targetDate;

        var report = await _salesRepository.GetSalesReportAsync(startDate, endDate);

        var dailyReport = new DailySalesReportDto(
            targetDate,
            report.Sum(r => r.Total),
            report.Count,
            new List<SaleDto>()
        );

        return Ok(dailyReport);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockReportDto>>> GetLowStock()
    {
        var products = await _productRepository.GetProductsAsync();

        var lowStock = products
            .Where(p => p.Quantity <= p.QuantityMin)
            .Select(p => new LowStockReportDto(
                p.Barcode,
                p.ProductName,
                p.Quantity,
                p.QuantityMin
            ))
            .ToList();

        return Ok(lowStock);
    }
}
