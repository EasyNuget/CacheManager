using CacheManager;
using RabbitMQ.Client;

namespace CacheManagerClear.Rabbit;

/// <summary>
/// Add RabbitMQ Cache Clear
/// </summary>
public static class RabbitCacheClearBuilder
{
	/// <summary>
	/// Use RabbitMQ Publisher
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="exchange">RabbitMQ exchange</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRabbitPublisher(this CacheManagerClearBuilder builder, IConnection connection, string exchange)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(connection);
		ArgumentNullException.ThrowIfNull(exchange);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (connection is null)
		{
			throw new ArgumentNullException(nameof(connection));
		}

		if (exchange is null)
		{
			throw new ArgumentNullException(nameof(exchange));
		}
#endif

		var publisher = new CachePublisher(connection, exchange);

		_ = builder.BuildPublisher(publisher);

		return builder;
	}

	/// <summary>
	/// Use RabbitMQ Subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="exchange">RabbitMQ exchange</param>
	/// <param name="queue">RabbitMQ queue</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRabbitSubscriber(this CacheManagerClearBuilder builder, IConnection connection, IEasyCacheManager cacheManager, string exchange, string queue)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(connection);
		ArgumentNullException.ThrowIfNull(exchange);
		ArgumentNullException.ThrowIfNull(queue);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (connection is null)
		{
			throw new ArgumentNullException(nameof(connection));
		}

		if (exchange is null)
		{
			throw new ArgumentNullException(nameof(exchange));
		}

		if (queue is null)
		{
			throw new ArgumentNullException(nameof(queue));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var subscriber = new CacheSubscriber(connection, exchange, queue, cacheManager);

		_ = builder.BuildSubscriber(subscriber);

		return builder;
	}

	/// <summary>
	/// Use RabbitMQ publisher and subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="exchange">RabbitMQ exchange</param>
	/// <param name="queue">RabbitMQ queue</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRabbitPublisherAndSubscriber(this CacheManagerClearBuilder builder, IConnection connection, IEasyCacheManager cacheManager, string exchange, string queue)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(connection);
		ArgumentNullException.ThrowIfNull(exchange);
		ArgumentNullException.ThrowIfNull(queue);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (connection is null)
		{
			throw new ArgumentNullException(nameof(connection));
		}

		if (exchange is null)
		{
			throw new ArgumentNullException(nameof(exchange));
		}

		if (queue is null)
		{
			throw new ArgumentNullException(nameof(queue));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var publisher = new CachePublisher(connection, exchange);
		var subscriber = new CacheSubscriber(connection, exchange, queue, cacheManager);

		_ = builder.BuildPublisherAndSubscriber(publisher, subscriber);

		return builder;
	}
}
