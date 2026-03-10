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
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IProductTypeRepository _productTypeRepository;

    public ProductsController(
        IProductRepository productRepository,
        IBrandRepository brandRepository,
        IProductTypeRepository productTypeRepository)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _productTypeRepository = productTypeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var products = await _productRepository.GetProductsAsync();
        return Ok(products.Select(MapToDto).ToList());
    }

    [HttpGet("{barcode}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string barcode)
    {
        var product = await _productRepository.GetProductByBarcodeAsync(barcode);
        if (product == null)
            return NotFound(new { message = $"Product with barcode {barcode} not found" });

        return Ok(MapToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var brand = (await _brandRepository.GetBrandsAsync()).FirstOrDefault(b => b.Id == dto.BrandId);
        var category = (await _productTypeRepository.GetProductTypesAsync()).FirstOrDefault(t => t.Id == dto.CategoryId);

        var product = new Product
        {
            Barcode = dto.Barcode,
            ProductName = dto.ProductName,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            QuantityMin = dto.QuantityMin,
            CostPrice = dto.CostPrice,
            RetailPrice = dto.RetailPrice,
            BrandId = dto.BrandId,
            BrandName = brand?.Name ?? "",
            CategoryId = dto.CategoryId,
            CategoryName = category?.Name ?? "",
            Status = dto.Status
        };

        var success = await _productRepository.AddProductAsync(product);
        if (!success)
            return BadRequest(new { message = "Failed to create product" });

        return CreatedAtAction(nameof(GetProduct), new { barcode = product.Barcode }, MapToDto(product));
    }

    [HttpPut("{barcode}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(string barcode, [FromBody] UpdateProductDto dto)
    {
        var brand = (await _brandRepository.GetBrandsAsync()).FirstOrDefault(b => b.Id == dto.BrandId);
        var category = (await _productTypeRepository.GetProductTypesAsync()).FirstOrDefault(t => t.Id == dto.CategoryId);

        var product = new Product
        {
            Barcode = barcode,
            ProductName = dto.ProductName,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            QuantityMin = dto.QuantityMin,
            CostPrice = dto.CostPrice,
            RetailPrice = dto.RetailPrice,
            BrandId = dto.BrandId,
            BrandName = brand?.Name ?? "",
            CategoryId = dto.CategoryId,
            CategoryName = category?.Name ?? "",
            Status = dto.Status
        };

        var success = await _productRepository.UpdateProductAsync(product);
        if (!success)
            return NotFound(new { message = "Product not found or failed to update" });

        return Ok(MapToDto(product));
    }

    [HttpDelete("{barcode}")]
    public async Task<IActionResult> DeleteProduct(string barcode)
    {
        var success = await _productRepository.DeleteProductAsync(barcode);
        if (!success)
            return NotFound(new { message = "Product not found" });

        return NoContent();
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Barcode,
        p.ProductName,
        p.Unit,
        p.Quantity,
        p.QuantityMin,
        p.CostPrice,
        p.RetailPrice,
        p.BrandId,
        p.BrandName,
        p.CategoryId,
        p.CategoryName,
        p.Status
    );
}
