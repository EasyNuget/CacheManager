namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with Get ability
/// </summary>
public interface ICacheSourceWithGet : ICacheSourceBase
{
	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	public Task<T?> GetAsync<T>(string key);
}
