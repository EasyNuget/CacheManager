namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with set ability
/// </summary>
/// <typeparam name="T">Item to cache</typeparam>
public interface ICacheSourceWithSet<T> : IBaseCacheSource<T>
{
    /// <summary>
    /// Set data to cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="data">Data to cache</param>
    Task SetAsync(string key, T data);
}