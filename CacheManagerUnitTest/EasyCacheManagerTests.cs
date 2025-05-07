using CacheManager;
using CacheManager.CacheSource;
using CacheManager.Config;
using Moq;

namespace CacheManagerUnitTest;

public class EasyCacheManagerTests : IAsyncLifetime
{
	private LockConfig _lockConfig = null!;

	public Task InitializeAsync()
	{
		_lockConfig = new LockConfig { PoolSize = 1, PoolInitialFill = 1, TimeOut = TimeSpan.FromSeconds(5) };

		return Task.CompletedTask;
	}

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public async Task SetAsync_ShouldClearAndSetCacheOnAllSources()
	{
		// Arrange
		const string Key = StaticData.Key;
		const string Value = StaticData.Key;

		var cacheSourceWithSet1 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet1.Setup(x => x.Priority).Returns(1);
		var cacheSourceWithSet2 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet2.Setup(x => x.Priority).Returns(2);

		var cacheSourceWithClear1 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear1.Setup(x => x.Priority).Returns(3);
		var cacheSourceWithClear2 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear2.Setup(x => x.Priority).Returns(4);

		var cacheSources = new List<ICacheSourceWithGet> { cacheSourceWithSet1.Object, cacheSourceWithSet2.Object, cacheSourceWithClear1.Object, cacheSourceWithClear2.Object };

		var cacheManager = new EasyCacheManager(cacheSources, _lockConfig);

		// Act
		await cacheManager.SetAsync(Key, Value).ConfigureAwait(true);

		// Assert
		cacheSourceWithSet1.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		cacheSourceWithSet2.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
	}

	[Fact]
	public async Task GetAsync_ShouldRetrieveFromCacheAndSetInHigherPrioritySources()
	{
		// Arrange
		const string Key = StaticData.Key;
		const string Value = StaticData.Key;

		var cacheSource1 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSource1.Setup(x => x.Priority).Returns(2);
		_ = cacheSource1.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync(Value);

		var cacheSource2 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSource2.Setup(x => x.Priority).Returns(1);
		_ = cacheSource2.Setup(x => x.GetAsync<string>(It.IsAny<string>())).ReturnsAsync((string)null!);

		var cacheSources = new List<ICacheSourceWithGet> { cacheSource1.Object, cacheSource2.Object };
		var cacheManager = new EasyCacheManager(cacheSources, _lockConfig);

		// Act
		var result = await cacheManager.GetAsync<string>(Key).ConfigureAwait(true);

		// Assert
		Assert.Equal(Value, result);
		cacheSource2.Verify(x => x.SetAsync(It.IsAny<string>(), Value), Times.Once);
	}

	[Fact]
	public void Constructor_ShouldThrowException_WhenCacheSourcesIsNull()
	{
		// Act & Assert
		_ = Assert.Throws<ArgumentException>(() => new EasyCacheManager(null!, _lockConfig));
	}

	[Fact]
	public void Constructor_ShouldThrowException_WhenLockConfigIsNull()
	{
		// Arrange
		var cacheSource = new Mock<ICacheSourceWithGet>().Object;
		var cacheSources = new List<ICacheSourceWithGet> { cacheSource };

		// Act & Assert
		_ = Assert.Throws<ArgumentException>(() => new EasyCacheManager(cacheSources, null!));
	}

	[Fact]
	public void Constructor_ShouldThrowException_WhenDuplicatePrioritiesExist()
	{
		// Arrange
		var cacheSource1 = new Mock<ICacheSourceWithGet>();
		_ = cacheSource1.Setup(x => x.Priority).Returns(1);
		var cacheSource2 = new Mock<ICacheSourceWithGet>();
		_ = cacheSource2.Setup(x => x.Priority).Returns(1);

		var cacheSources = new List<ICacheSourceWithGet> { cacheSource1.Object, cacheSource2.Object };

		// Act & Assert
		var exception = Assert.Throws<ArgumentException>(() => new EasyCacheManager(cacheSources, _lockConfig));
		Assert.Contains("Duplicate priority values found", exception.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetAsync_ShouldGetFromApiAndSetToOther_ValidData()
	{
		// Arrange
		const string Key = StaticData.Key;
		const string Value = StaticData.Key;

		var memoryCacheSource = new Mock<ICacheSourceWithGetWithSetAndClear>();
		var redisCacheSource = new Mock<ICacheSourceWithGetWithSetAndClear>();
		var dbCacheSource = new Mock<ICacheSourceWithGet>();
		var apiCacheSource = new Mock<ICacheSourceWithGet>();

		_ = memoryCacheSource.Setup(x => x.GetAsync<string>(Key)).ReturnsAsync((string)null!);
		_ = memoryCacheSource.Setup(x => x.SetAsync(Key, Value));
		_ = memoryCacheSource.Setup(x => x.ClearAsync(Key));
		_ = memoryCacheSource.Setup(x => x.Priority).Returns(1);

		_ = redisCacheSource.Setup(x => x.GetAsync<string>(Key)).ReturnsAsync((string)null!);
		_ = redisCacheSource.Setup(x => x.SetAsync(Key, Value));
		_ = redisCacheSource.Setup(x => x.ClearAsync(Key));
		_ = redisCacheSource.Setup(x => x.Priority).Returns(2);

		_ = dbCacheSource.Setup(x => x.GetAsync<string>(Key)).ReturnsAsync((string)null!);
		_ = dbCacheSource.Setup(x => x.Priority).Returns(3);

		_ = apiCacheSource.Setup(x => x.GetAsync<string>(Key)).ReturnsAsync(Value);
		_ = apiCacheSource.Setup(x => x.Priority).Returns(4);

		var cacheSources = new List<ICacheSourceWithGet> { apiCacheSource.Object, dbCacheSource.Object, memoryCacheSource.Object, redisCacheSource.Object };

		// Act
		var easyCacheManager = new EasyCacheManager(cacheSources, _lockConfig);
		var result = await easyCacheManager.GetAsync<string>(Key).ConfigureAwait(true);

		//Assert
		Assert.Equal(Value, result);

		memoryCacheSource.Verify(x => x.GetAsync<string>(Key), Times.Once, "Memory GetAsync should call only one");
		redisCacheSource.Verify(x => x.GetAsync<string>(Key), Times.Once, "Redis GetAsync should call only one");
		dbCacheSource.Verify(x => x.GetAsync<string>(Key), Times.Once, "Db GetAsync should call only one");
		apiCacheSource.Verify(x => x.GetAsync<string>(Key), Times.Once, "API GetAsync should call only one");

		memoryCacheSource.Verify(x => x.SetAsync(Key, Value), Times.Once, "Memory SetAsync should call only one");
		redisCacheSource.Verify(x => x.SetAsync(Key, Value), Times.Once, "Redis SetAsync should call only one");

		memoryCacheSource.Verify(x => x.ClearAsync(Key), Times.Never, "Memory ClearAsync should not call");
		redisCacheSource.Verify(x => x.ClearAsync(Key), Times.Never, "Redis ClearAsync should not call");
	}

	[Fact]
	public async Task StopAsync_CallsStopOnAllCacheSources()
	{
		// Arrange
		var cacheSourceWithSet1 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet1.Setup(x => x.Priority).Returns(1);
		_ = cacheSourceWithSet1.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithSet2 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet2.Setup(x => x.Priority).Returns(2);
		_ = cacheSourceWithSet2.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithClear1 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear1.Setup(x => x.Priority).Returns(3);
		_ = cacheSourceWithClear1.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithClear2 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear2.Setup(x => x.Priority).Returns(4);
		_ = cacheSourceWithClear2.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSources = new List<ICacheSourceWithGet> { cacheSourceWithSet1.Object, cacheSourceWithSet2.Object, cacheSourceWithClear1.Object, cacheSourceWithClear2.Object };

		var cacheManager = new EasyCacheManager(cacheSources, _lockConfig);

		// Act
		await cacheManager.StopAsync().ConfigureAwait(false);

		// Assert
		cacheSourceWithSet1.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithSet2.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithClear1.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithClear2.Verify(c => c.StopAsync(), Times.Once);
	}

	[Fact]
	public async Task DisposeAsync_CallsStopAsyncForAllSources()
	{
		// Arrange
		var cacheSourceWithSet1 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet1.Setup(x => x.Priority).Returns(1);
		_ = cacheSourceWithSet1.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithSet2 = new Mock<ICacheSourceWithGetWithSet>();
		_ = cacheSourceWithSet2.Setup(x => x.Priority).Returns(2);
		_ = cacheSourceWithSet2.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithClear1 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear1.Setup(x => x.Priority).Returns(3);
		_ = cacheSourceWithClear1.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSourceWithClear2 = new Mock<ICacheSourceWithGetWithClear>();
		_ = cacheSourceWithClear2.Setup(x => x.Priority).Returns(4);
		_ = cacheSourceWithClear2.Setup(x => x.StopAsync()).Returns(Task.CompletedTask);

		var cacheSources = new List<ICacheSourceWithGet> { cacheSourceWithSet1.Object, cacheSourceWithSet2.Object, cacheSourceWithClear1.Object, cacheSourceWithClear2.Object };

		var cacheManager = new EasyCacheManager(cacheSources, _lockConfig);

		// Act
		await cacheManager.DisposeAsync().ConfigureAwait(true);

		// Assert
		cacheSourceWithSet1.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithSet2.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithClear1.Verify(c => c.StopAsync(), Times.Once);
		cacheSourceWithClear2.Verify(c => c.StopAsync(), Times.Once);
	}
}
