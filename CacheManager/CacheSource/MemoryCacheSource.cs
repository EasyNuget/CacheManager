using CacheManager.Config;
using Microsoft.Extensions.Caching.Memory;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Memory
/// </summary>
public class MemoryCacheSource : ICacheSourceWithGetWithSetAndClear
{
	private readonly MemoryCache _memoryCache;
	private readonly MemoryConfig _config;

	/// <summary>
	/// Create Get from Api
	/// </summary>
	/// <param name="config">Api Config</param>
	/// <param name="priority">Priority</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public MemoryCacheSource(MemoryConfig config, int priority)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
		_memoryCache = new MemoryCache(new MemoryCacheOptions());
	}

	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	public Task<T?> GetAsync<T>(string key)
	{
		_ = _memoryCache.TryGetValue(key, out T? value);
		return Task.FromResult(value);
	}

	/// <summary>
	/// Set data to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="data">Data to cache</param>
	public Task SetAsync<T>(string key, T data)
	{
		_ = _memoryCache.Set(key, data, _config.CacheTime);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Clear from cache with key
	/// </summary>
	/// <param name="key"></param>
	public Task ClearAsync(string key)
	{
		_memoryCache.Remove(key);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Clear all from cache
	/// </summary>
	public Task ClearAllAsync()
	{
		_memoryCache.Clear();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Stops
	/// </summary>
	public Task StopAsync()
	{
		_memoryCache.Dispose();

		return Task.CompletedTask;
	}

	/// <summary>
	/// Dispose
	/// </summary>
	/// <returns></returns>
	public async ValueTask DisposeAsync()
	{
		await StopAsync().ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
#if NET8_0_OR_GREATER
	public int Priority { get; init; }
#else
	public int Priority { get; set; }
#endif
}
