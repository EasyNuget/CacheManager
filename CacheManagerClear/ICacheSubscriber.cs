namespace CacheManagerClear;

/// <summary>
/// Subscriber for cache clear event
/// </summary>
public interface ICacheSubscriber : IAsyncDisposable
{
	/// <summary>
	/// Subscriber for cache clear event
	/// </summary>
	Task SubscribeAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Stops the subscription process
	/// </summary>
	/// <returns></returns>
	Task StopAsync();
}
