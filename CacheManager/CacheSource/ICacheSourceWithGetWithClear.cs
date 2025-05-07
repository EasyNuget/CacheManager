namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with clear ability
/// </summary>
public interface ICacheSourceWithGetWithClear : ICacheSourceWithGet
{
	/// <summary>
	/// Clear from cache with key
	/// </summary>
	/// <param name="key"></param>
	public Task ClearAsync(string key);

	/// <summary>
	/// Clear all from cache
	/// </summary>
	public Task ClearAllAsync();
}
