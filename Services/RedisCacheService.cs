using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
namespace ExcelUploadPortal.Services;
public class RedisCacheService
{
    private readonly IDistributedCache _cache;
    
    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    // Save data to Redis
    public async Task SetCacheAsync<T>(string key, T value, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        
        var jsonData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonData, options);
    }

    // Retrieve data from Redis
    public async Task<T?> GetCacheAsync<T>(string key)
    {
        var jsonData = await _cache.GetStringAsync(key);
        return jsonData is null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }

    // Remove cache entry
    public async Task RemoveCacheAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
