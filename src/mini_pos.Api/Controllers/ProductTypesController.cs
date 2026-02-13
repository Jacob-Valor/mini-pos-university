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
public class ProductTypesController : ControllerBase
{
    private readonly IProductTypeRepository _productTypeRepository;

    public ProductTypesController(IProductTypeRepository productTypeRepository)
    {
        _productTypeRepository = productTypeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductTypeDto>>> GetProductTypes()
    {
        var types = await _productTypeRepository.GetProductTypesAsync();
        return Ok(types.Select(MapToDto).ToList());
    }

    private static ProductTypeDto MapToDto(ProductType pt) => new(pt.Id, pt.Name);
}
