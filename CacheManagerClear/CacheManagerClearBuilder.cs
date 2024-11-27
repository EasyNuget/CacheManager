namespace CacheManagerClear;

/// <summary>
/// Builder for CacheManagerClear
/// </summary>
public class CacheManagerClearBuilder
{
	/// <summary>
	/// Builds the CacheManagerClear Publisher.
	/// </summary>
	/// <returns>Clear event publisher</returns>
	/// <exception cref="InvalidOperationException">If publisher is null.</exception>
	public ICachePublisher BuildPublisher(ICachePublisher publisher)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(publisher);
#else
		if (publisher is null)
		{
			throw new ArgumentNullException(nameof(publisher));
		}
#endif

		return publisher;
	}

	/// <summary>
	/// Builds the CacheManagerClear Publisher.
	/// </summary>
	/// <returns>Clear event subscriber</returns>
	/// <exception cref="InvalidOperationException">If subscriber is null.</exception>
	public ICacheSubscriber BuildSubscriber(ICacheSubscriber subscriber)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(subscriber);
#else
		if (subscriber is null)
		{
			throw new ArgumentNullException(nameof(subscriber));
		}
#endif

		return subscriber;
	}

	/// <summary>
	/// Builds the CacheManagerClear Publisher and Subscriber.
	/// </summary>
	/// <param name="publisher">Publisher</param>
	/// <param name="subscriber">Subscriber</param>
	/// <returns></returns>
	public (ICachePublisher, ICacheSubscriber) BuildPublisherAndSubscriber(ICachePublisher publisher, ICacheSubscriber subscriber) => (BuildPublisher(publisher), BuildSubscriber(subscriber));
}
