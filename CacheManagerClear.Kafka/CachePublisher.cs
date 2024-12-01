using Confluent.Kafka;

namespace CacheManagerClear.Kafka;

/// <summary>
/// Send event to clear cache by kafka
/// </summary>
public class CachePublisher : ICachePublisher
{
	private readonly IProducer<Null, string> _producer;
	private readonly string _topic;
	private bool _disposed;

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
	/// <param name="cancellationToken">CancellationToken</param>
	public async Task PublishClearCacheAsync(string key, CancellationToken? cancellationToken)
	{
		var message = new Message<Null, string> { Value = key };
		_ = await _producer.ProduceAsync(_topic, message, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
	}

	/// <summary>
	/// Publish a clear all cache event to Kafka
	/// </summary>
	public async Task PublishClearAllCacheAsync(CancellationToken? cancellationToken)
	{
		var message = new Message<Null, string> { Value = StaticData.ClearAllKey };
		_ = await _producer.ProduceAsync(_topic, message, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
	}

	/// <summary>
	/// Stops the Kafka subscription process.
	/// </summary>
	public Task StopAsync()
	{
		_producer.Dispose();

		return Task.CompletedTask;
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
