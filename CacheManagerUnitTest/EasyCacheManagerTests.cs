using CacheManager;
using CacheManager.CacheSource;
using CacheManager.Config;
using Moq;

namespace CacheManagerUnitTest;

public class EasyCacheManagerTests
{
    private readonly LockConfig _lockConfig;

    public EasyCacheManagerTests()
    {
        _lockConfig = new LockConfig
        {
            PoolSize = 1,
            PoolInitialFill = 1,
            MaxCount = 1,
            TimeOut = TimeSpan.FromSeconds(5)
        };
    }

    [Fact]
    public async Task SetAsync_ShouldClearAndSetCacheOnAllSources()
    {
        // Arrange
        const string key = "testKey";
        const string value = "testValue";

        var cacheSourceWithSet1 = new Mock<ICacheSourceWithSet<string>>();
        cacheSourceWithSet1.Setup(x => x.Priority).Returns(1);
        var cacheSourceWithSet2 = new Mock<ICacheSourceWithSet<string>>();
        cacheSourceWithSet2.Setup(x => x.Priority).Returns(2);

        var cacheSourceWithClear1 = new Mock<ICacheSourceWithClear<string>>();
        cacheSourceWithClear1.Setup(x => x.Priority).Returns(3);
        var cacheSourceWithClear2 = new Mock<ICacheSourceWithClear<string>>();
        cacheSourceWithClear2.Setup(x => x.Priority).Returns(4);

        var cacheSources = new List<IBaseCacheSource<string>>
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
        cacheSourceWithClear1.Verify(x => x.ClearAsync(It.IsAny<string>()), Times.Once);
        cacheSourceWithClear2.Verify(x => x.ClearAsync(It.IsAny<string>()), Times.Once);
        cacheSourceWithSet1.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        cacheSourceWithSet2.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldRetrieveFromCacheAndSetInHigherPrioritySources()
    {
        // Arrange
        const string key = "testKey";
        const string value = "testValue";

        var cacheSource1 = new Mock<ICacheSourceWithSet<string>>();
        cacheSource1.Setup(x => x.Priority).Returns(2);
        cacheSource1.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(value);

        var cacheSource2 = new Mock<ICacheSourceWithSet<string>>();
        cacheSource2.Setup(x => x.Priority).Returns(1);
        cacheSource2.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((string)null);

        var cacheSources = new List<IBaseCacheSource<string>> { cacheSource1.Object, cacheSource2.Object };
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
        var cacheSource = new Mock<IBaseCacheSource<string>>().Object;
        var cacheSources = new List<IBaseCacheSource<string>> { cacheSource };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new EasyCacheManager<string>(cacheSources, null));
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenDuplicatePrioritiesExist()
    {
        // Arrange
        var cacheSource1 = new Mock<IBaseCacheSource<string>>();
        cacheSource1.Setup(x => x.Priority).Returns(1);
        var cacheSource2 = new Mock<IBaseCacheSource<string>>();
        cacheSource2.Setup(x => x.Priority).Returns(1);

        var cacheSources = new List<IBaseCacheSource<string>> { cacheSource1.Object, cacheSource2.Object };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new EasyCacheManager<string>(cacheSources, _lockConfig));
        Assert.Contains("Duplicate priority values found", exception.Message);
    }
}