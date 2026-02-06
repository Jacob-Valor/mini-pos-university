using System.Threading.Tasks;

namespace mini_pos.Services;

public interface IEmployeeCredentialsRepository
{
    Task<bool> UpdatePasswordAsync(string empId, string newPasswordHash);
    Task<string?> GetStoredPasswordHashAsync(string username);
}
