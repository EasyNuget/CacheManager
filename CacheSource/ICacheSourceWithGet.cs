namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with Get ability
/// </summary>
/// <typeparam name="T">Item to cache</typeparam>
public interface ICacheSourceWithGet<T> : ICacheSourceBase
{
    /// <summary>
    /// Get from cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Result</returns>
    Task<T?> GetAsync(string key);
}