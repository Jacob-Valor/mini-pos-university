using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace mini_pos.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, DateTime> _expiryTimes = new();

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            return Task.FromResult(value);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (expiry.HasValue)
        {
            cacheEntryOptions.SetAbsoluteExpiration(expiry.Value);
            _expiryTimes[key] = DateTime.UtcNow.Add(expiry.Value);
        }
        else
        {
            cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            _expiryTimes[key] = DateTime.UtcNow.AddMinutes(5);
        }

        _cache.Set(key, value, cacheEntryOptions);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        _expiryTimes.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}
