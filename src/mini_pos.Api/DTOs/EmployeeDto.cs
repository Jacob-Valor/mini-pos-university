using System;

namespace mini_pos.Api.DTOs;

public record EmployeeDto(
    string EmpId,
    string EmpName,
    string EmpLname,
    string Gender,
    DateTime DateOfBirth,
    string VillageId,
    string Tel,
    DateTime StartDate,
    string Position,
    string Status
);

public record LoginRequestDto(
    string Username,
    string Password
);

public record LoginResponseDto(
    bool Success,
    string? Message,
    EmployeeDto? Employee
);
