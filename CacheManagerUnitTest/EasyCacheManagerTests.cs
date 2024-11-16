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
        _lockConfig = new LockConfig
        {
            PoolSize = 1,
            PoolInitialFill = 1,
            MaxCount = 1,
            TimeOut = TimeSpan.FromSeconds(5)
        };

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SetAsync_ShouldClearAndSetCacheOnAllSources()
    {
        // Arrange
        const string key = StaticData.Key;
        const string value = StaticData.Key;

        var cacheSourceWithSet1 = new Mock<ICacheSourceWithGetWithSet<string>>();
        cacheSourceWithSet1.Setup(x => x.Priority).Returns(1);
        var cacheSourceWithSet2 = new Mock<ICacheSourceWithGetWithSet<string>>();
        cacheSourceWithSet2.Setup(x => x.Priority).Returns(2);

        var cacheSourceWithClear1 = new Mock<ICacheSourceWithGetWithClear<string>>();
        cacheSourceWithClear1.Setup(x => x.Priority).Returns(3);
        var cacheSourceWithClear2 = new Mock<ICacheSourceWithGetWithClear<string>>();
        cacheSourceWithClear2.Setup(x => x.Priority).Returns(4);

        var cacheSources = new List<ICacheSourceWithGet<string>>
        {
            cacheSourceWithSet1.Object,
            cacheSourceWithSet2.Object,
            cacheSourceWithClear1.Object,
            cacheSourceWithClear2.Object
        };

        var cacheManager = new EasyCacheManager<string>(cacheSources, _lockConfig);

        // Act
        await cacheManager.SetAsync(key, value);

        // Assert
        cacheSourceWithSet1.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        cacheSourceWithSet2.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldRetrieveFromCacheAndSetInHigherPrioritySources()
    {
        // Arrange
        const string key = StaticData.Key;
        const string value = StaticData.Key;

        var cacheSource1 = new Mock<ICacheSourceWithGetWithSet<string>>();
        cacheSource1.Setup(x => x.Priority).Returns(2);
        cacheSource1.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(value);

        var cacheSource2 = new Mock<ICacheSourceWithGetWithSet<string>>();
        cacheSource2.Setup(x => x.Priority).Returns(1);
        cacheSource2.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((string)null!);

        var cacheSources = new List<ICacheSourceWithGet<string>> { cacheSource1.Object, cacheSource2.Object };
        var cacheManager = new EasyCacheManager<string>(cacheSources, _lockConfig);

        // Act
        var result = await cacheManager.GetAsync(key);

        // Assert
        Assert.Equal(value, result);
        cacheSource2.Verify(x => x.SetAsync(It.IsAny<string>(), value), Times.Once);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenCacheSourcesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EasyCacheManager<string>(null, _lockConfig));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenLockConfigIsNull()
    {
        // Arrange
        var cacheSource = new Mock<ICacheSourceWithGet<string>>().Object;
        var cacheSources = new List<ICacheSourceWithGet<string>> { cacheSource };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EasyCacheManager<string>(cacheSources, null));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDuplicatePrioritiesExist()
    {
        // Arrange
        var cacheSource1 = new Mock<ICacheSourceWithGet<string>>();
        cacheSource1.Setup(x => x.Priority).Returns(1);
        var cacheSource2 = new Mock<ICacheSourceWithGet<string>>();
        cacheSource2.Setup(x => x.Priority).Returns(1);

        var cacheSources = new List<ICacheSourceWithGet<string>> { cacheSource1.Object, cacheSource2.Object };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EasyCacheManager<string>(cacheSources, _lockConfig));
        Assert.Contains("Duplicate priority values found", exception.Message);
    }

    [Fact]
    public async Task GetAsync_ShouldGetFromApiAndSetToOther_ValidData()
    {
        // Arrange
        const string key = StaticData.Key;
        const string value = StaticData.Key;

        var memoryCacheSource = new Mock<ICacheSourceWithGetWithSetAndClear<string>>();
        var redisCacheSource = new Mock<ICacheSourceWithGetWithSetAndClear<string>>();
        var dbCacheSource = new Mock<ICacheSourceWithGet<string>>();
        var apiCacheSource = new Mock<ICacheSourceWithGet<string>>();

        memoryCacheSource.Setup(x => x.GetAsync(key)).ReturnsAsync((string)null!);
        memoryCacheSource.Setup(x => x.SetAsync(key, value));
        memoryCacheSource.Setup(x => x.ClearAsync(key));
        memoryCacheSource.Setup(x => x.Priority).Returns(1);

        redisCacheSource.Setup(x => x.GetAsync(key)).ReturnsAsync((string)null!);
        redisCacheSource.Setup(x => x.SetAsync(key, value));
        redisCacheSource.Setup(x => x.ClearAsync(key));
        redisCacheSource.Setup(x => x.Priority).Returns(2);

        dbCacheSource.Setup(x => x.GetAsync(key)).ReturnsAsync((string)null!);
        dbCacheSource.Setup(x => x.Priority).Returns(3);

        apiCacheSource.Setup(x => x.GetAsync(key)).ReturnsAsync(value);
        apiCacheSource.Setup(x => x.Priority).Returns(4);

        var cacheSources = new List<ICacheSourceWithGet<string>>
        {
            apiCacheSource.Object,
            dbCacheSource.Object,
            memoryCacheSource.Object,
            redisCacheSource.Object
        };

        // Act
        var easyCacheManager = new EasyCacheManager<string>(cacheSources, _lockConfig);
        var result = await easyCacheManager.GetAsync(key);

        //Assert
        Assert.Equal(value, result);

        memoryCacheSource.Verify(x => x.GetAsync(key), Times.Once, "Memory GetAsync should call only one");
        redisCacheSource.Verify(x => x.GetAsync(key), Times.Once, "Redis GetAsync should call only one");
        dbCacheSource.Verify(x => x.GetAsync(key), Times.Once, "Db GetAsync should call only one");
        apiCacheSource.Verify(x => x.GetAsync(key), Times.Once, "API GetAsync should call only one");

        memoryCacheSource.Verify(x => x.SetAsync(key, value), Times.Once, "Memory SetAsync should call only one");
        redisCacheSource.Verify(x => x.SetAsync(key, value), Times.Once, "Redis SetAsync should call only one");

        memoryCacheSource.Verify(x => x.ClearAsync(key), Times.Never, "Memory ClearAsync should not call");
        redisCacheSource.Verify(x => x.ClearAsync(key), Times.Never, "Redis ClearAsync should not call");
    }
}