using CacheManager;
using Confluent.Kafka;

namespace CacheManagerClear.Kafka;

/// <summary>
/// Subscriber for cache clear event by kafka
/// </summary>
public class CacheSubscriber : ICacheSubscriber
{
	private CancellationTokenSource? _cancellationTokenSource;
	private readonly IConsumer<Null, string> _consumer;
	private readonly IEasyCacheManager _cacheManager;
	private readonly string _topic;
	private Task _task = null!;
	private bool _disposed;

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
	public Task SubscribeAsync(CancellationToken cancellationToken)
	{
		if (_task != null)
		{
			throw new InvalidOperationException(Resources.SubscriptionRunning);
		}

		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_consumer.Subscribe(_topic);

		_task = Task.Run(() => _ = ConsumeAsync(cancellationToken), cancellationToken);

		return Task.CompletedTask;
	}

	/// <summary>
	/// Stops the Kafka subscription process.
	/// </summary>
	public async Task StopAsync()
	{
#if NET8_0_OR_GREATER
		await _cancellationTokenSource!.CancelAsync().ConfigureAwait(false);
		await _task.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
#else
		_cancellationTokenSource?.Cancel();
		_task.Wait(TimeSpan.FromSeconds(10));
#endif

		_consumer.Unsubscribe();
		_consumer.Close();
		_consumer.Dispose();
		_cancellationTokenSource?.Dispose();
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

	private async Task ConsumeAsync(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				var consumeResult = _consumer.Consume(cancellationToken);

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
			catch (Exception e)
			{
				Console.WriteLine(e);

				await Task.Delay(100, cancellationToken).ConfigureAwait(false);
			}
		}
	}
}
