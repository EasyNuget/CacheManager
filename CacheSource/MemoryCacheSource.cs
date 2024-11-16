using CacheManager.Config;
using Microsoft.Extensions.Caching.Memory;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Memory
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class MemoryCacheSource<T> : ICacheSourceWithGetWithSetAndClear<T>
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryConfig _config;

    /// <summary>
    /// Create Get from Api
    /// </summary>
    /// <param name="config">Api Config</param>
    /// <param name="priority">Priority</param>
    /// <exception cref="ArgumentException">Config is null</exception>
    public MemoryCacheSource(MemoryConfig config, int priority)
    {
        Priority = priority;
        _config = config ?? throw new ArgumentException("Config is null", nameof(config));
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Get from cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Result</returns>
    public Task<T?> GetAsync(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    /// <summary>
    /// Set data to cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="data">Data to cache</param>
    public Task SetAsync(string key, T data)
    {
        _memoryCache.Set(key, data, _config.CacheTime);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clear from cache with key
    /// </summary>
    /// <param name="key"></param>
    public Task ClearAsync(string key)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Priority, Lowest priority - checked last
    /// </summary>
#if NETSTANDARD2_0 || NET462
    public int Priority { get; set; }
#else
     public int Priority { get; init; }
#endif
}