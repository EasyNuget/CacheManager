using System.Diagnostics.CodeAnalysis;
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
	private bool _disposed;

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
	[SuppressMessage("Style", "IDE0060:Remove unused parameter")]
	public async Task SubscribeAsync(CancellationToken cancellationToken)
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

	/// <summary>
	/// Stops the Kafka subscription process.
	/// </summary>
	public async Task StopAsync()
	{
		await _redis.DisposeAsync().ConfigureAwait(false);
		await _redis.CloseAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Disposes the Kafka consumer and cancels the subscription.
	/// </summary>
	/// <returns></returns>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		await StopAsync().ConfigureAwait(false);

		_disposed = true;
	}
}
