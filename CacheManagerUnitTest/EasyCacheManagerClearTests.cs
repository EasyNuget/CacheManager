using CacheManagerClear;
using Moq;

namespace CacheManagerUnitTest;

public class EasyCacheManagerClearTests : IAsyncLifetime
{
	private CacheManagerClearBuilder _builder = null!;

	public Task InitializeAsync()
	{
		_builder = new CacheManagerClearBuilder();

		return Task.CompletedTask;
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void BuildPublisher_ValidPublisher_ReturnsSamePublisher()
	{
		// Arrange
		var mockPublisher = new Mock<ICachePublisher>().Object;

		// Act
		var result = _builder.BuildPublisher(mockPublisher);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(mockPublisher, result);
	}

	[Fact]
	public void BuildPublisher_NullPublisher_ThrowsArgumentNullException()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => _builder.BuildPublisher(null!));
	}

	[Fact]
	public void BuildSubscriber_ValidSubscriber_ReturnsSameSubscriber()
	{
		// Arrange
		var mockSubscriber = new Mock<ICacheSubscriber>().Object;

		// Act
		var result = _builder.BuildSubscriber(mockSubscriber);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(mockSubscriber, result);
	}

	[Fact]
	public void BuildSubscriber_NullSubscriber_ThrowsArgumentNullException()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => _builder.BuildSubscriber(null!));
	}

	[Fact]
	public void BuildPublisherAndSubscriber_ValidInputs_ReturnsPublisherAndSubscriber()
	{
		// Arrange
		var mockPublisher = new Mock<ICachePublisher>().Object;
		var mockSubscriber = new Mock<ICacheSubscriber>().Object;

		// Act
		var (publisherResult, subscriberResult) = _builder.BuildPublisherAndSubscriber(mockPublisher, mockSubscriber);

		// Assert
		Assert.NotNull(publisherResult);
		Assert.NotNull(subscriberResult);
		Assert.Equal(mockPublisher, publisherResult);
		Assert.Equal(mockSubscriber, subscriberResult);
	}

	[Fact]
	public void BuildPublisherAndSubscriber_NullPublisher_ThrowsArgumentNullException()
	{
		// Arrange
		var mockSubscriber = new Mock<ICacheSubscriber>().Object;

		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => _builder.BuildPublisherAndSubscriber(null!, mockSubscriber));
	}

	[Fact]
	public void BuildPublisherAndSubscriber_NullSubscriber_ThrowsArgumentNullException()
	{
		// Arrange
		var mockPublisher = new Mock<ICachePublisher>().Object;

		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => _builder.BuildPublisherAndSubscriber(mockPublisher, null!));
	}

	[Fact]
	public void BuildPublisherAndSubscriber_NullInputs_ThrowsArgumentNullException()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentNullException>(() => _builder.BuildPublisherAndSubscriber(null!, null!));
	}
}
