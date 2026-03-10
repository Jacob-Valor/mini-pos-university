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
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        var customers = await _customerRepository.GetCustomersAsync();
        return Ok(customers.Select(MapToDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(string id)
    {
        var customers = await _customerRepository.GetCustomersAsync();
        var customer = customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(MapToDto(customer));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Id = GenerateCustomerId(),
            Name = dto.CusName,
            Surname = dto.CusLname,
            Gender = dto.Gender,
            Address = dto.Address,
            PhoneNumber = dto.Tel ?? ""
        };

        var success = await _customerRepository.AddCustomerAsync(customer);
        if (!success)
            return BadRequest(new { message = "Failed to create customer" });

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, MapToDto(customer));
    }

    private static string GenerateCustomerId()
    {
        return $"CUS{DateTime.Now.Ticks % 10000000:D7}";
    }

    private static CustomerDto MapToDto(Customer c) => new(
        c.Id,
        c.Name,
        c.Surname,
        c.Gender,
        c.Address,
        c.PhoneNumber
    );
}
