namespace mini_pos.Api.DTOs;

public record ProductDto(
    string Barcode,
    string ProductName,
    string Unit,
    int Quantity,
    int QuantityMin,
    decimal CostPrice,
    decimal RetailPrice,
    string BrandId,
    string BrandName,
    string CategoryId,
    string CategoryName,
    string Status
);

public record CreateProductDto(
    string Barcode,
    string ProductName,
    string Unit,
    int Quantity,
    int QuantityMin,
    decimal CostPrice,
    decimal RetailPrice,
    string BrandId,
    string CategoryId,
    string Status
);

public record UpdateProductDto(
    string ProductName,
    string Unit,
    int Quantity,
    int QuantityMin,
    decimal CostPrice,
    decimal RetailPrice,
    string BrandId,
    string CategoryId,
    string Status
);
