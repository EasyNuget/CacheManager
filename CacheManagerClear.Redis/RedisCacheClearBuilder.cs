using CacheManager;
using StackExchange.Redis;

namespace CacheManagerClear.Redis;

/// <summary>
/// Add Redis Cache Clear
/// </summary>
public static class RedisCacheClearBuilder
{
	/// <summary>
	/// Use Redis Publisher
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="redis">Redis</param>
	/// <param name="channel">Redis channel</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRedisPublisher(this CacheManagerClearBuilder builder, IConnectionMultiplexer redis, string channel)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(redis);
		ArgumentNullException.ThrowIfNull(channel);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (redis is null)
		{
			throw new ArgumentNullException(nameof(redis));
		}

		if (channel is null)
		{
			throw new ArgumentNullException(nameof(channel));
		}
#endif

		var publisher = new CachePublisher(redis, channel);

		_ = builder.BuildPublisher(publisher);

		return builder;
	}

	/// <summary>
	/// Use Redis Subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="redis">Redis</param>
	/// <param name="channel">Redis channel</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRedisSubscriber(this CacheManagerClearBuilder builder, IConnectionMultiplexer redis, string channel, IEasyCacheManager cacheManager)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(redis);
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (redis is null)
		{
			throw new ArgumentNullException(nameof(redis));
		}

		if (channel is null)
		{
			throw new ArgumentNullException(nameof(channel));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var subscriber = new CacheSubscriber(redis, channel, cacheManager);

		_ = builder.BuildSubscriber(subscriber);

		return builder;
	}

	/// <summary>
	/// Use Redis Subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="redis">Redis</param>
	/// <param name="channel">Redis channel</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseRedisPublisherAndSubscriber(this CacheManagerClearBuilder builder, IConnectionMultiplexer redis, string channel, IEasyCacheManager cacheManager)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(redis);
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (redis is null)
		{
			throw new ArgumentNullException(nameof(redis));
		}

		if (channel is null)
		{
			throw new ArgumentNullException(nameof(channel));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var publisher = new CachePublisher(redis, channel);
		var subscriber = new CacheSubscriber(redis, channel, cacheManager);

		_ = builder.BuildPublisherAndSubscriber(publisher, subscriber);

		return builder;
	}
}
