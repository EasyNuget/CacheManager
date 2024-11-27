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
	Task ClearAsync(string key);

	/// <summary>
	/// Clear all from cache
	/// </summary>
	Task ClearAllAsync();
}
