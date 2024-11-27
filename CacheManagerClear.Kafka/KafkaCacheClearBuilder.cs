using CacheManager;
using Confluent.Kafka;

namespace CacheManagerClear.Kafka;

/// <summary>
/// Add Kafka Cache Clear
/// </summary>
public static class KafkaCacheClearBuilder
{
	/// <summary>
	/// Use Kafka Publisher
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="producer">Kafka producer</param>
	/// <param name="topic">Kafka Topic</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseKafkaPublisher(this CacheManagerClearBuilder builder, IProducer<Null, string> producer, string topic)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(producer);
		ArgumentNullException.ThrowIfNull(topic);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (producer is null)
		{
			throw new ArgumentNullException(nameof(producer));
		}

		if (topic is null)
		{
			throw new ArgumentNullException(nameof(topic));
		}
#endif

		var publisher = new CachePublisher(producer, topic);

		_ = builder.BuildPublisher(publisher);

		return builder;
	}

	/// <summary>
	/// Use Kafka Subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="consumer">Kafka consumer</param>
	/// <param name="topic">Kafka topic</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseKafkaSubscriber(this CacheManagerClearBuilder builder, IConsumer<Null, string> consumer, string topic, IEasyCacheManager cacheManager)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(consumer);
		ArgumentNullException.ThrowIfNull(topic);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (consumer is null)
		{
			throw new ArgumentNullException(nameof(consumer));
		}

		if (topic is null)
		{
			throw new ArgumentNullException(nameof(topic));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var subscriber = new CacheSubscriber(consumer, topic, cacheManager);

		_ = builder.BuildSubscriber(subscriber);

		return builder;
	}

	/// <summary>
	/// Use Kafka Subscriber
	/// </summary>
	/// <param name="builder">CacheManagerClearBuilder</param>
	/// <param name="consumer">Kafka consumer</param>
	/// <param name="producer">Kafka producer</param>
	/// <param name="topic">Kafka topic</param>
	/// <param name="cacheManager">IEasyCacheManager</param>
	/// <returns></returns>
	public static CacheManagerClearBuilder UseKafkaPublisherAndSubscriber(this CacheManagerClearBuilder builder, IProducer<Null, string> producer, IConsumer<Null, string> consumer, string topic,
		IEasyCacheManager cacheManager)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(consumer);
		ArgumentNullException.ThrowIfNull(topic);
		ArgumentNullException.ThrowIfNull(cacheManager);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (consumer is null)
		{
			throw new ArgumentNullException(nameof(consumer));
		}

		if (topic is null)
		{
			throw new ArgumentNullException(nameof(topic));
		}

		if (cacheManager is null)
		{
			throw new ArgumentNullException(nameof(cacheManager));
		}
#endif

		var publisher = new CachePublisher(producer, topic);
		var subscriber = new CacheSubscriber(consumer, topic, cacheManager);

		_ = builder.BuildPublisherAndSubscriber(publisher, subscriber);

		return builder;
	}
}
