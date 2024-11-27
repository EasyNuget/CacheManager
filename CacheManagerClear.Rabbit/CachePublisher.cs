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
	public async Task PublishClearCacheAsync(string key) => await PublishMessage(key).ConfigureAwait(false);

	/// <summary>
	/// Publishes a clear all cache event.
	/// </summary>
	public async Task PublishClearAllCacheAsync() => await PublishMessage(StaticData.ClearAllKey).ConfigureAwait(false);

	private async Task PublishMessage(string message)
	{
		using var channel = await _connection.CreateChannelAsync().ConfigureAwait(false);

		// Declare exchange
		await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Fanout, durable: true).ConfigureAwait(false);

		var body = Encoding.UTF8.GetBytes(message);

		// Publish message
		await channel.BasicPublishAsync(exchange: _exchange, routingKey: string.Empty, body: body).ConfigureAwait(false);
	}
}
