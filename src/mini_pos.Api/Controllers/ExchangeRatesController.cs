using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mini_pos.Api.DTOs;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateRepository _exchangeRateRepository;

    public ExchangeRatesController(IExchangeRateRepository exchangeRateRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ExchangeRateDto>> GetLatest()
    {
        var rate = await _exchangeRateRepository.GetLatestExchangeRateAsync();
        if (rate == null)
            return NotFound(new { message = "No exchange rate found" });

        return Ok(MapToDto(rate));
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeRateDto>> UpdateRate([FromBody] UpdateExchangeRateDto dto)
    {
        var rate = new ExchangeRate
        {
            UsdRate = dto.Dolar,
            ThbRate = dto.Bath,
            CreatedDate = DateTime.Now
        };

        var success = await _exchangeRateRepository.AddExchangeRateAsync(rate);
        if (!success)
            return BadRequest(new { message = "Failed to update exchange rate" });

        return Ok(MapToDto(rate));
    }

    private static ExchangeRateDto MapToDto(ExchangeRate er) => new(
        er.Id,
        er.UsdRate,
        er.ThbRate,
        er.CreatedDate
    );
}
