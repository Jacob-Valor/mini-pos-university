using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using mini_pos.Api.DTOs;
using mini_pos.Api.Services;
using mini_pos.Models;

namespace mini_pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesApplicationService _salesService;

    public SalesController(ISalesApplicationService salesService)
    {
        _salesService = salesService;
    }

    [HttpGet("report")]
    public async Task<ActionResult<List<SalesReportItem>>> GetSalesReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _salesService.GetSalesReportAsync(startDate, endDate);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Items);
    }

    [HttpPost]
    public async Task<ActionResult> CreateSale([FromBody] CreateSaleDto dto)
    {
        var result = await _salesService.CreateSaleAsync(dto);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Sale created successfully" });
    }
}
