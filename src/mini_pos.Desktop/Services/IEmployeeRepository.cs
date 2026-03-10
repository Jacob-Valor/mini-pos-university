using System.Collections.Generic;
using System.Threading.Tasks;

using mini_pos.Models;

namespace mini_pos.Services;

public interface IEmployeeRepository
{
    Task<Employee?> GetEmployeeByUsernameAsync(string username);
    Task<List<Employee>> GetEmployeesAsync();
    Task<bool> AddEmployeeAsync(Employee emp);
    Task<bool> UpdateEmployeeAsync(Employee emp);
    Task<bool> DeleteEmployeeAsync(string empId);
    Task<bool> UpdateEmployeeProfileAsync(Employee emp);
}
