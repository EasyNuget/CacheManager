namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public interface IEasyCacheManager : IAsyncDisposable
{
	/// <summary>
	/// Get Cached item from all. if not exist call api and returned it, also cached it
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>cached item</returns>
	Task<T?> GetAsync<T>(string key);

	/// <summary>
	/// Manual set value to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <returns>cached item</returns>
	Task SetAsync<T>(string key, T value);

	/// <summary>
	/// Clear cached item by key
	/// </summary>
	/// <param name="key">Key</param>
	Task ClearCacheAsync(string key);

	/// <summary>
	/// Clear cached item from all
	/// </summary>
	Task ClearAllCacheAsync();
}
