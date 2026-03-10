using System;
using System.Collections.Generic;

namespace mini_pos.Api.DTOs;

public record SaleDto(
    int SaleId,
    int ExId,
    string CusId,
    string EmpId,
    DateTime DateSale,
    decimal Subtotal,
    decimal Pay,
    decimal MoneyChange
);

public record SaleDetailDto(
    int Id,
    int SalesId,
    string ProductId,
    int Qty,
    decimal Price,
    decimal Total
);

public record CreateSaleDto(
    int ExId,
    string CusId,
    string EmpId,
    decimal Subtotal,
    decimal Pay,
    decimal MoneyChange,
    List<CreateSaleItemDto> Items
);

public record CreateSaleItemDto(
    string ProductId,
    int Qty,
    decimal Price,
    decimal Total
);

public record SaleWithDetailsDto(
    SaleDto Sale,
    List<SaleDetailDto> Details
);
