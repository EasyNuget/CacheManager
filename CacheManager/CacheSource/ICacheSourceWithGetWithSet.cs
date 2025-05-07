namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with set ability
/// </summary>
public interface ICacheSourceWithGetWithSet : ICacheSourceWithGet
{
	/// <summary>
	/// Set data to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="data">Data to cache</param>
	public Task SetAsync<T>(string key, T data);
}
