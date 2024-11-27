using Confluent.Kafka;

namespace CacheManagerClear.Kafka;

/// <summary>
/// Send event to clear cache by kafka
/// </summary>
public class CachePublisher : ICachePublisher
{
	private readonly IProducer<Null, string> _producer;
	private readonly string _topic;

	/// <summary>
	/// Send event to clear cache by kafka
	/// </summary>
	/// <param name="producer">Kafka</param>
	/// <param name="topic">Topic</param>
	/// <exception cref="ArgumentNullException">producer is null</exception>
	/// <exception cref="ArgumentNullException">topic is null</exception>
	public CachePublisher(IProducer<Null, string> producer, string topic)
	{
		_producer = producer ?? throw new ArgumentNullException(nameof(producer));
		_topic = topic ?? throw new ArgumentNullException(nameof(topic));
	}

	/// <summary>
	/// Publish a cache clear event by key to Kafka
	/// </summary>
	/// <param name="key">Cache key</param>
	public async Task PublishClearCacheAsync(string key)
	{
		var message = new Message<Null, string> { Value = key };
		_ = await _producer.ProduceAsync(_topic, message).ConfigureAwait(false);
	}

	/// <summary>
	/// Publish a clear all cache event to Kafka
	/// </summary>
	public async Task PublishClearAllCacheAsync()
	{
		var message = new Message<Null, string> { Value = StaticData.ClearAllKey };
		_ = await _producer.ProduceAsync(_topic, message).ConfigureAwait(false);
	}
}
