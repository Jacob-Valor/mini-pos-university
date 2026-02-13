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
public class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _supplierRepository;

    public SuppliersController(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _supplierRepository.GetSuppliersAsync();
        return Ok(suppliers.Select(MapToDto).ToList());
    }

    private static SupplierDto MapToDto(Supplier s) => new(
        s.Id,
        s.Name,
        s.ContactName,
        s.Email,
        s.Phone,
        s.Address
    );
}
