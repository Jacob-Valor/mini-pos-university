using System.Collections.Generic;
using System.Threading.Tasks;
using mini_pos.Models;

namespace mini_pos.Services;

public interface IGeoRepository
{
    Task<List<Province>> GetProvincesAsync();
    Task<List<District>> GetDistrictsByProvinceAsync(string provinceId);
    Task<List<Village>> GetVillagesByDistrictAsync(string districtId);
}
