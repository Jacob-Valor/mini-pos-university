using System.Collections.Generic;
using System.Threading.Tasks;

using mini_pos.Models;

namespace mini_pos.Services;

public interface IProductTypeRepository
{
    Task<List<ProductType>> GetProductTypesAsync();
    Task<bool> AddProductTypeAsync(ProductType type);
    Task<bool> UpdateProductTypeAsync(ProductType type);
    Task<bool> DeleteProductTypeAsync(string typeId);
}
