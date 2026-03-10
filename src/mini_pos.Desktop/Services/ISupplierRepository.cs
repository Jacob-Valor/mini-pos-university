using System.Collections.Generic;
using System.Threading.Tasks;

using mini_pos.Models;

namespace mini_pos.Services;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetSuppliersAsync();
    Task<bool> AddSupplierAsync(Supplier supplier);
    Task<bool> UpdateSupplierAsync(Supplier supplier);
    Task<bool> DeleteSupplierAsync(string supplierId);
}
