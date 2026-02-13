using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;

namespace mini_pos.Services;

public interface IBrandRepository
{
    Task<List<Brand>> GetBrandsAsync();
    Task<bool> AddBrandAsync(Brand brand);
    Task<bool> UpdateBrandAsync(Brand brand);
    Task<bool> DeleteBrandAsync(string brandId);
}
