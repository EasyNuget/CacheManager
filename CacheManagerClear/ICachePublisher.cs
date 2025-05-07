namespace CacheManagerClear;

/// <summary>
/// Send event to clear cache
/// </summary>
public interface ICachePublisher : IAsyncDisposable
{
	/// <summary>
	/// Publish clear cached event by key
	/// </summary>
	/// <param name="key">key</param>
	/// <param name="cancellationToken">CancellationToken</param>
	/// <returns></returns>
	public Task PublishClearCacheAsync(string key, CancellationToken? cancellationToken);

	/// <summary>
	/// Publish clear all cached event
	/// </summary>
	/// <returns></returns>
	public Task PublishClearAllCacheAsync(CancellationToken? cancellationToken);

	/// <summary>
	/// Stops the Kafka subscription process
	/// </summary>
	/// <returns></returns>
	public Task StopAsync();
}
