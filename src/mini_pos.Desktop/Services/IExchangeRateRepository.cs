using System.Collections.Generic;
using System.Threading.Tasks;

using mini_pos.Models;

namespace mini_pos.Services;

public interface IExchangeRateRepository
{
    Task<ExchangeRate?> GetLatestExchangeRateAsync();
    Task<List<ExchangeRate>> GetExchangeRateHistoryAsync();
    Task<bool> AddExchangeRateAsync(ExchangeRate rate);
}
