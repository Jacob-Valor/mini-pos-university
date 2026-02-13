using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mini_pos.Api.DTOs;
using mini_pos.Models;
using mini_pos.Services;

namespace mini_pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeesController(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees()
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        return Ok(employees.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(string id)
    {
        var employees = await _employeeRepository.GetEmployeesAsync();
        var employee = employees.FirstOrDefault(e => e.Id == id);
        if (employee == null)
            return NotFound(new { message = $"Employee with ID {id} not found" });

        return Ok(MapToDto(employee));
    }

    private static EmployeeDto MapToDto(Employee e) => new(
        e.Id,
        e.Name,
        e.Surname,
        e.Gender,
        e.DateOfBirth,
        e.VillageId,
        e.PhoneNumber,
        DateTime.Now,
        e.Position,
        "active"
    );
}
