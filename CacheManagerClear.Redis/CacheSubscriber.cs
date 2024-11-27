using CacheManager;
using StackExchange.Redis;

namespace CacheManagerClear.Redis;

/// <summary>
/// Subscriber for cache clear event by redis pop/sub
/// </summary>
public class CacheSubscriber : ICacheSubscriber
{
	private readonly string _channel;
	private readonly IConnectionMultiplexer _redis;
	private readonly IEasyCacheManager _cacheManager;

	/// <summary>
	/// Subscriber for cache clear event by redis pop/sub
	/// </summary>
	/// <param name="redis">Redis</param>
	/// <param name="channel">RedisChannel</param>
	/// <param name="cacheManager">CacheManager</param>
	/// <exception cref="ArgumentNullException">redis is null</exception>
	/// <exception cref="ArgumentNullException">channel is null</exception>
	/// <exception cref="ArgumentNullException">cacheManager is null</exception>
	public CacheSubscriber(IConnectionMultiplexer redis, string channel, IEasyCacheManager cacheManager)
	{
		_redis = redis ?? throw new ArgumentNullException(nameof(redis));
		_cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
		_channel = channel ?? throw new ArgumentNullException(nameof(channel));
	}

	/// <summary>
	/// Subscriber for cache clear event
	/// </summary>
	public async Task SubscribeAsync()
	{
		var subscriber = _redis.GetSubscriber();

		var channel = RedisChannel.Literal(_channel);

		await subscriber.SubscribeAsync(channel, async (_, message) =>
		{
			var key = message.ToString();

			if (key.Equals(StaticData.ClearAllKey, StringComparison.Ordinal))
			{
				await _cacheManager.ClearAllCacheAsync().ConfigureAwait(false);
			}
			else
			{
				await _cacheManager.ClearCacheAsync(key).ConfigureAwait(false);
			}
		}).ConfigureAwait(false);
	}
}
