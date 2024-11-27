namespace CacheManagerClear;

/// <summary>
/// Subscriber for cache clear event
/// </summary>
public interface ICacheSubscriber
{
	/// <summary>
	/// Subscriber for cache clear event
	/// </summary>
	Task SubscribeAsync();
}
