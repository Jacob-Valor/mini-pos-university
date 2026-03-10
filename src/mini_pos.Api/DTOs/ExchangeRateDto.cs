using System;

namespace mini_pos.Api.DTOs;

public record ExchangeRateDto(
    int Id,
    decimal Dolar,
    decimal Bath,
    DateTime ExDate
);

public record UpdateExchangeRateDto(
    decimal Dolar,
    decimal Bath
);
