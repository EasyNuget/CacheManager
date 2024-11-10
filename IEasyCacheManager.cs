namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public interface IEasyCacheManager<T>
{
    /// <summary>
    /// Get Cached item from: memory, redis, db. if not exist call api and returned it, also cached it
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>cached item</returns>
    Task<T?> GetAsync(string key);

    /// <summary>
    /// Clear cached item from memory and redis with key
    /// </summary>
    /// <param name="key">Key</param>
    Task ClearCacheAsync(string key);
}