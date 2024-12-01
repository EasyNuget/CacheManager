using CacheManager;
using CacheManagerClear.Kafka;
using Confluent.Kafka;
using Moq;

namespace CacheManagerUnitTest;

public class EasyCacheManagerClearKafkaTests : IAsyncLifetime
{
	private Mock<IProducer<Null, string>> _mockProducer = null!;
	private CachePublisher _cachePublisher = null!;
	private Mock<IConsumer<Null, string>> _mockConsumer = null!;
	private Mock<IEasyCacheManager> _mockCacheManager = null!;
	private CacheSubscriber _cacheSubscriber = null!;

	public Task InitializeAsync()
	{
		_mockProducer = new Mock<IProducer<Null, string>>();

		_cachePublisher = new CachePublisher(_mockProducer.Object, StaticData.Topic);
		_ = _mockProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new DeliveryResult<Null, string>());

		_mockConsumer = new Mock<IConsumer<Null, string>>();
		_mockCacheManager = new Mock<IEasyCacheManager>();
		_cacheSubscriber = new CacheSubscriber(_mockConsumer.Object, StaticData.Topic, _mockCacheManager.Object);

		return Task.CompletedTask;
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void Constructor_ValidProducerAndTopic_CreatesCachePublisher()
	{
		// Act
		var result = new CachePublisher(_mockProducer.Object, StaticData.Topic);

		// Assert
		Assert.NotNull(result);
	}

	[Fact]
	public void Constructor_NullProducer_ThrowsArgumentNullException()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => new CachePublisher(null!, StaticData.Topic));
	}

	[Fact]
	public void Constructor_NullTopic_ThrowsArgumentNullException()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => new CachePublisher(_mockProducer.Object, null!));
	}

	[Fact]
	public async Task PublishClearCacheAsync_ValidKey_CallsProduceAsync()
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		// Act
		await _cachePublisher.PublishClearCacheAsync(StaticData.Key, cancellationTokenSource.Token).ConfigureAwait(true);

		// Assert
		_mockProducer.Verify(p => p.ProduceAsync(StaticData.Topic, It.Is<Message<Null, string>>(m => m.Value == StaticData.Key), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task PublishClearAllCacheAsync_CallsProduceAsync()
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

		// Act
		await _cachePublisher.PublishClearAllCacheAsync(cancellationTokenSource.Token).ConfigureAwait(true);

		// Assert
		_mockProducer.Verify(p => p.ProduceAsync(StaticData.Topic, It.Is<Message<Null, string>>(m => m.Value == StaticData.ClearAllKey), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task SubscribeAsync_ValidKey_CallsClearCacheAsync()
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		var consumeResult = new ConsumeResult<Null, string> { Message = new Message<Null, string> { Value = StaticData.Key } };

		// Set up the consumer to return the mock consume result
		_ = _mockConsumer.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

		// Act
		await _cacheSubscriber.SubscribeAsync(cancellationTokenSource.Token).ConfigureAwait(true);

		// Assert
		_mockCacheManager.Verify(c => c.ClearCacheAsync(StaticData.Key), Times.AtLeastOnce);
		_mockCacheManager.Verify(c => c.ClearAllCacheAsync(), Times.Never);
	}

	[Fact]
	public async Task SubscribeAsync_ClearAllKey_CallsClearAllCacheAsync()
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		var consumeResult = new ConsumeResult<Null, string> { Message = new Message<Null, string> { Value = StaticData.ClearAllKey } };

		// Set up the consumer to return the mock consume result
		_ = _mockConsumer.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Returns(consumeResult);

		// Act
		await _cacheSubscriber.SubscribeAsync(cancellationTokenSource.Token).ConfigureAwait(true);

		// Assert
		_mockCacheManager.Verify(c => c.ClearAllCacheAsync(), Times.AtLeastOnce);
		_mockCacheManager.Verify(c => c.ClearCacheAsync(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task SubscribeAsync_ConsumeException_DoesNotThrow()
	{
		// Arrange
		var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
		_ = _mockConsumer.Setup(c => c.Consume(It.IsAny<CancellationToken>())).Throws<Exception>();

		// Act & Assert
		await _cacheSubscriber.SubscribeAsync(cancellationTokenSource.Token).ConfigureAwait(true);

		// No exceptions should be thrown
		_mockConsumer.Verify(c => c.Consume(It.IsAny<CancellationToken>()), Times.Once);
	}
}
