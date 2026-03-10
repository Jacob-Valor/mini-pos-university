using System.Collections.Generic;
using System.Threading.Tasks;

using mini_pos.Models;

namespace mini_pos.Services;

public interface IProductRepository
{
    Task<List<Product>> GetProductsAsync();
    Task<Product?> GetProductByBarcodeAsync(string barcode);
    Task<bool> ProductExistsAsync(string barcode);
    Task<bool> AddProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string barcode);
}
