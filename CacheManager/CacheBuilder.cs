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
	/// <param name="priority">Default is 1</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddMemory(MemoryConfig memoryConfig, int priority = 1)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(memoryConfig);
#else
		if (memoryConfig is null)
		{
			throw new ArgumentNullException(nameof(memoryConfig));
		}
#endif

		_cacheSources.Add(new MemoryCacheSource(memoryConfig, priority));
		return this;
	}

	/// <summary>
	/// Add Api Cache
	/// </summary>
	/// <param name="apiConfig">Config</param>
	/// <param name="priority">Default is 2</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddApi(ApiConfig apiConfig, int priority = 2)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(apiConfig);
#else
		if (apiConfig is null)
		{
			throw new ArgumentNullException(nameof(apiConfig));
		}
#endif

		_cacheSources.Add(new ApiCacheSourceWithGet(apiConfig, priority));

		return this;
	}

	/// <summary>
	/// Add custom cache source
	/// </summary>
	/// <param name="sourceWithGet">Your custom source</param>
	/// <returns>CacheBuilder</returns>
	public CacheBuilder AddCustom(ICacheSourceWithGet sourceWithGet)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(sourceWithGet);
#else
		if (sourceWithGet is null)
		{
			throw new ArgumentNullException(nameof(sourceWithGet));
		}
#endif

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
