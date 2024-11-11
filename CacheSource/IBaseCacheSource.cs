namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider
/// </summary>
/// <typeparam name="T">Item to cache</typeparam>
public interface IBaseCacheSource<T>
{
    /// <summary>
    /// Get from cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Result</returns>
    Task<T?> GetAsync(string key);

    /// <summary>
    /// Priority, Lowest priority - checked last
    /// </summary>
    int Priority { get; }
}