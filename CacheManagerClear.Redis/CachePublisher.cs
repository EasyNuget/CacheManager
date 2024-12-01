using StackExchange.Redis;

namespace CacheManagerClear.Redis;

/// <summary>
/// Send event to clear cache by redis pop/sub
/// </summary>
public class CachePublisher : ICachePublisher
{
	private readonly string _channel;
	private readonly IConnectionMultiplexer _redis;
	private bool _disposed;

	/// <summary>
	/// Send event to clear cache by redis pop/sub
	/// </summary>
	/// <param name="redis">Redis</param>
	/// <param name="channel">Redis channel</param>
	/// <exception cref="ArgumentNullException">redis is null</exception>
	/// <exception cref="ArgumentNullException">channel is null</exception>
	public CachePublisher(IConnectionMultiplexer redis, string channel)
	{
		_redis = redis ?? throw new ArgumentNullException(nameof(redis));
		_channel = channel ?? throw new ArgumentNullException(nameof(channel));
	}

	/// <summary>
	/// Publish clear cached event by key
	/// </summary>
	/// <param name="key">key</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	public async Task PublishClearCacheAsync(string key, CancellationToken? cancellationToken)
	{
		var channel = _redis.GetSubscriber();
		var redisChannel = RedisChannel.Literal(_channel);

		_ = await channel.PublishAsync(redisChannel, key).ConfigureAwait(false);
	}

	/// <summary>
	/// Publish clear all cached event
	/// </summary>
	/// <returns></returns>
	public async Task PublishClearAllCacheAsync(CancellationToken? cancellationToken)
	{
		var channel = _redis.GetSubscriber();
		var redisChannel = RedisChannel.Pattern(_channel);

		_ = await channel.PublishAsync(redisChannel, StaticData.ClearAllKey).ConfigureAwait(false);
	}

	/// <summary>
	/// Stops the Kafka subscription process
	/// </summary>
	public async Task StopAsync()
	{
		await _redis.DisposeAsync().ConfigureAwait(false);
		await _redis.CloseAsync().ConfigureAwait(false);
	}

	/// <summary>
	/// Disposes the Kafka consumer and cancels the subscription
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
