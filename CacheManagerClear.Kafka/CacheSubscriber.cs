using CacheManager;
using Confluent.Kafka;

namespace CacheManagerClear.Kafka;

/// <summary>
/// Subscriber for cache clear event by kafka
/// </summary>
public class CacheSubscriber : ICacheSubscriber
{
	private readonly IConsumer<Null, string> _consumer;
	private readonly IEasyCacheManager _cacheManager;
	private readonly string _topic;

	/// <summary>
	/// Subscriber for cache clear event by kafka
	/// </summary>
	/// <param name="consumer">Kafka</param>
	/// <param name="topic">Topic</param>
	/// <param name="cacheManager">Cache Manager</param>
	/// <exception cref="ArgumentNullException">consumer is null</exception>
	/// <exception cref="ArgumentNullException">cacheManager is null</exception>
	/// <exception cref="ArgumentNullException">topic is null</exception>
	public CacheSubscriber(IConsumer<Null, string> consumer, string topic, IEasyCacheManager cacheManager)
	{
		_consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
		_cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
		_topic = topic ?? throw new ArgumentNullException(nameof(topic));
	}

	/// <summary>
	/// Start the Kafka subscriber to listen for cache clear events
	/// </summary>
	public async Task SubscribeAsync()
	{
		_consumer.Subscribe(_topic);

		try
		{
			while (true)
			{
				var consumeResult = await Task.Run(() => _consumer.Consume()).ConfigureAwait(false);

				var key = consumeResult.Message.Value;

				if (key.Equals(StaticData.ClearAllKey, StringComparison.Ordinal))
				{
					await _cacheManager.ClearAllCacheAsync().ConfigureAwait(false);
				}
				else
				{
					await _cacheManager.ClearCacheAsync(key).ConfigureAwait(false);
				}
			}
		}
		catch (ConsumeException ex)
		{
			Console.WriteLine($"Error consuming Kafka message: {ex.Error.Reason}");
		}
	}
}
