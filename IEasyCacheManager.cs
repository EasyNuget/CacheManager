namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public interface IEasyCacheManager<T> : IAsyncDisposable
{
	/// <summary>
	/// Get Cached item from all. if not exist call api and returned it, also cached it
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>cached item</returns>
	Task<T?> GetAsync(string key);

	/// <summary>
	/// Manual set value to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <returns>cached item</returns>
	Task SetAsync(string key, T value);

	/// <summary>
	/// Clear cached item from all
	/// </summary>
	/// <param name="key">Key</param>
	Task ClearCacheAsync(string key);
}
