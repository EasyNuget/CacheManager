namespace CacheManagerClear;

/// <summary>
/// Send event to clear cache
/// </summary>
public interface ICachePublisher
{
	/// <summary>
	/// Publish clear cached event by key
	/// </summary>
	/// <param name="key">key</param>
	/// <returns></returns>
	Task PublishClearCacheAsync(string key);

	/// <summary>
	/// Publish clear all cached event
	/// </summary>
	/// <returns></returns>
	Task PublishClearAllCacheAsync();
}
