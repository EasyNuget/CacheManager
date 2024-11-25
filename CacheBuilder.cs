using CacheManager.CacheSource;
using CacheManager.Config;

namespace CacheManager;

/// <summary>
/// Simple cache builder
/// </summary>
public class CacheBuilder
{
	private readonly List<ICacheSourceWithGet> _cacheSources = [];

	/// <summary>
	/// Add Memory Cache
	/// </summary>
	/// <param name="memoryConfig">Config</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddMemory(MemoryConfig memoryConfig)
	{
		_cacheSources.Add(new MemoryCacheSource(memoryConfig, 1));
		return this;
	}

	/// <summary>
	/// Add Redis Cache
	/// </summary>
	/// <param name="redisConfig">Config</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddRedis(RedisConfig redisConfig)
	{
		_cacheSources.Add(new RedisCacheSource(redisConfig, 2));
		return this;
	}

	/// <summary>
	/// Add Sql Server Cache
	/// </summary>
	/// <param name="dbConfig">Config</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddDb(DbConfig dbConfig)
	{
		_cacheSources.Add(new DbCacheSourceWithGet(dbConfig, 3));
		return this;
	}

	/// <summary>
	/// Add Api Cache
	/// </summary>
	/// <param name="apiConfig">Config</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddApi(ApiConfig apiConfig)
	{
		_cacheSources.Add(new ApiCacheSourceWithGet(apiConfig, 4));
		return this;
	}

	/// <summary>
	/// Add custom cache source
	/// </summary>
	/// <param name="sourceWithGet">Your custom source</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddCustom(ICacheSourceWithGet sourceWithGet)
	{
		_cacheSources.Add(sourceWithGet);
		return this;
	}

	/// <summary>
	/// Build EasyCacheManager
	/// </summary>
	/// <param name="lockConfig">Config</param>
	/// <returns>EasyCacheManager</returns>
	public EasyCacheManager Build(LockConfig lockConfig) => new(_cacheSources, lockConfig);
}
