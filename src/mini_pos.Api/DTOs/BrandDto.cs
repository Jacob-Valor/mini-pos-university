namespace mini_pos.Api.DTOs;

public record BrandDto(
    string Id,
    string Name
);

public record CreateBrandDto(
    string Id,
    string Name
);
