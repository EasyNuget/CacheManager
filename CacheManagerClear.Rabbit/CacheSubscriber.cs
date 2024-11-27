using System.Text;
using CacheManager;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CacheManagerClear.Rabbit;

/// <summary>
/// Subscriber for cache clear event by RabbitMQ
/// </summary>
public class CacheSubscriber : ICacheSubscriber
{
	private readonly IConnection _connection;
	private readonly IEasyCacheManager _cacheManager;
	private readonly string _exchange;
	private readonly string _queue;

	/// <summary>
	/// Subscriber for cache clear event by RabbitMQ
	/// </summary>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="cacheManager">Cache manager instance</param>
	/// <param name="exchange">RabbitMQ exchange name</param>
	/// <param name="queue">RabbitMQ queue name</param>
	public CacheSubscriber(IConnection connection, IEasyCacheManager cacheManager, string exchange, string queue)
	{
		_connection = connection ?? throw new ArgumentNullException(nameof(connection));
		_cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
		_exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
		_queue = queue ?? throw new ArgumentNullException(nameof(queue));
	}

	/// <summary>
	/// Subscribes to cache clear events and processes them.
	/// </summary>
	public async Task SubscribeAsync()
	{
		using var channel = await _connection.CreateChannelAsync().ConfigureAwait(false);

		await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true).ConfigureAwait(false);
		_ = await channel.QueueDeclareAsync(queue: _queue, durable: true, exclusive: false, autoDelete: false).ConfigureAwait(false);
		await channel.QueueBindAsync(queue: _queue, exchange: _exchange, routingKey: string.Empty).ConfigureAwait(false);

		var consumer = new AsyncEventingBasicConsumer(channel);

		consumer.ReceivedAsync += async (_, ea) =>
		{
			var message = Encoding.UTF8.GetString(ea.Body.ToArray());

			if (message == "*")
			{
				await _cacheManager.ClearAllCacheAsync().ConfigureAwait(false);
			}
			else
			{
				await _cacheManager.ClearCacheAsync(message).ConfigureAwait(false);
			}

			await channel.BasicAckAsync(ea.DeliveryTag, false).ConfigureAwait(false);
		};

		// Start consuming messages
		_ = await channel.BasicConsumeAsync(queue: _queue, autoAck: false, consumer: consumer).ConfigureAwait(false);
	}
}
