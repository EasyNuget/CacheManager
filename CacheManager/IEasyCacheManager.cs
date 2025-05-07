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
	public Task<T?> GetAsync<T>(string key);

	/// <summary>
	/// Manual set value to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <returns>cached item</returns>
	public Task SetAsync<T>(string key, T value);

	/// <summary>
	/// Clear cached item by key
	/// </summary>
	/// <param name="key">Key</param>
	public Task ClearCacheAsync(string key);

	/// <summary>
	/// Clear cached item from all
	/// </summary>
	public Task ClearAllCacheAsync();

	/// <summary>
	/// Stop all cache sources
	/// </summary>
	/// <returns></returns>
	public Task StopAsync();
}
