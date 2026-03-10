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
public class BrandsController : ControllerBase
{
    private readonly IBrandRepository _brandRepository;

    public BrandsController(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> GetBrands()
    {
        var brands = await _brandRepository.GetBrandsAsync();
        return Ok(brands.Select(MapToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandDto dto)
    {
        var brand = new Brand { Id = dto.Id, Name = dto.Name };
        var success = await _brandRepository.AddBrandAsync(brand);
        if (!success)
            return BadRequest(new { message = "Failed to create brand" });

        return CreatedAtAction(nameof(GetBrands), new { id = brand.Id }, MapToDto(brand));
    }

    private static BrandDto MapToDto(Brand b) => new(b.Id, b.Name);
}
