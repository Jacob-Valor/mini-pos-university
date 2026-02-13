namespace mini_pos.Api.DTOs;

public record ProductTypeDto(
    string Id,
    string Name
);

public record CreateProductTypeDto(
    string Id,
    string Name
);
