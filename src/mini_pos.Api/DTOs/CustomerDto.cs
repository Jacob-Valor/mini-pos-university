using System;

namespace mini_pos.Api.DTOs;

public record CustomerDto(
    string CusId,
    string CusName,
    string CusLname,
    string Gender,
    string Address,
    string? Tel
);

public record CreateCustomerDto(
    string CusName,
    string CusLname,
    string Gender,
    string Address,
    string? Tel
);

public record UpdateCustomerDto(
    string CusName,
    string CusLname,
    string Gender,
    string Address,
    string? Tel
);
