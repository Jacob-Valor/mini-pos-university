namespace mini_pos.Api.DTOs;

public record SupplierDto(
    string SupId,
    string SupName,
    string ContractName,
    string Email,
    string Telephone,
    string Address
);

public record CreateSupplierDto(
    string SupId,
    string SupName,
    string ContractName,
    string Email,
    string Telephone,
    string Address
);
