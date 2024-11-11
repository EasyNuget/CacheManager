using System.Text.Json;
using CacheManager.Config;
using StackExchange.Redis;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Redis
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class RedisCacheSource<T> : ICacheSourceWithSetAndClear<T>
{
    private readonly IDatabase _redisCache;
    private readonly RedisConfig _config;

    /// <summary>
    /// Create Get from Api
    /// </summary>
    /// <param name="config">Api Config</param>
    /// <exception cref="ArgumentException">Config is null</exception>
    public RedisCacheSource(RedisConfig config)
    {
        _config = config ?? throw new ArgumentException("Config is null", nameof(config));
        var connectionMultiplexer = ConnectionMultiplexer.Connect(config.ConnectionString);
        _redisCache = connectionMultiplexer.GetDatabase();
    }

    /// <summary>
    /// Get from cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Result</returns>
    public async Task<T?> GetAsync(string key)
    {
        var value = await _redisCache.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    /// <summary>
    /// Set data to cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="data">Data to cache</param>
    public Task SetAsync(string key, T data)
    {
        return _redisCache.StringSetAsync(key, JsonSerializer.Serialize(data), _config.CacheTime);
    }

    /// <summary>
    /// Clear from cache with key
    /// </summary>
    /// <param name="key"></param>
    public Task ClearAsync(string key)
    {
        return _redisCache.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Priority, Lowest priority - checked last
    /// </summary>
    public int Priority => 2;
}