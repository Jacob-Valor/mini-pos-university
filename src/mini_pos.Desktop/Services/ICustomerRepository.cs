using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;

namespace mini_pos.Services;

public interface ICustomerRepository
{
    Task<List<Customer>> GetCustomersAsync();
    Task<List<Customer>> SearchCustomersAsync(string keyword);
    Task<bool> AddCustomerAsync(Customer customer);
    Task<bool> UpdateCustomerAsync(Customer customer);
    Task<bool> DeleteCustomerAsync(string customerId);
}
