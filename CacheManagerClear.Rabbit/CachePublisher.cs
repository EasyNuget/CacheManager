using System.Text;
using RabbitMQ.Client;

namespace CacheManagerClear.Rabbit;

/// <summary>
/// Send event to clear cache by kafka
/// </summary>
public class CachePublisher : ICachePublisher
{
	private readonly IConnection _connection;
	private readonly string _exchange;
	private bool _disposed;

	/// <summary>
	/// Send event to clear cache by RabbitMQ
	/// </summary>
	/// <param name="connection">RabbitMQ connection</param>
	/// <param name="exchange">RabbitMQ exchange name</param>
	public CachePublisher(IConnection connection, string exchange)
	{
		_connection = connection ?? throw new ArgumentNullException(nameof(connection));
		_exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
	}

	/// <summary>
	/// Publishes a cache clear event for a specific key.
	/// </summary>
	/// <param name="key">Cache key to clear</param>
	/// <param name="cancellationToken">CancellationToken</param>
	public async Task PublishClearCacheAsync(string key, CancellationToken? cancellationToken) => await PublishMessage(key, cancellationToken).ConfigureAwait(false);

	/// <summary>
	/// Stops the Kafka subscription process.
	/// </summary>
	public async Task StopAsync()
	{
		await _connection.DisposeAsync().ConfigureAwait(false);
		await _connection.CloseAsync().ConfigureAwait(false);
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

	/// <summary>
	/// Publishes a clear all cache event.
	/// </summary>
	public async Task PublishClearAllCacheAsync(CancellationToken? cancellationToken) => await PublishMessage(StaticData.ClearAllKey, cancellationToken).ConfigureAwait(false);

	private async Task PublishMessage(string message, CancellationToken? cancellationToken)
	{
		using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

		// Declare exchange
		await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

		var body = Encoding.UTF8.GetBytes(message);

		// Publish message
		await channel.BasicPublishAsync(exchange: _exchange, routingKey: string.Empty, body: body, cancellationToken: cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
	}
}
