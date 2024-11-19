using System.Text.Json;
using CacheManager.Config;
using StackExchange.Redis;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Redis
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class RedisCacheSource<T> : ICacheSourceWithGetWithSetAndClear<T>
{
	private readonly IDatabase _redisCache;
	private readonly RedisConfig _config;

	/// <summary>
	/// Create Get from Api
	/// </summary>
	/// <param name="config">Api Config</param>
	/// <param name="priority">Priority</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public RedisCacheSource(RedisConfig config, int priority)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
		var connectionMultiplexer = ConnectionMultiplexer.Connect(config.ConnectionString);
		_redisCache = connectionMultiplexer.GetDatabase();
	}

	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	public async Task<T?> GetAsync(string key)
	{
		var value = await _redisCache.StringGetAsync(key).ConfigureAwait(false);
		return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
	}

	/// <summary>
	/// Set data to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="data">Data to cache</param>
	public async Task SetAsync(string key, T data)
	{
		_ = await _redisCache.StringSetAsync(key, JsonSerializer.Serialize(data), _config.CacheTime).ConfigureAwait(false);
	}

	/// <summary>
	/// Clear from cache with key
	/// </summary>
	/// <param name="key"></param>
	public async Task ClearAsync(string key)
	{
		_ = await _redisCache.KeyDeleteAsync(key).ConfigureAwait(false);
	}

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public int Priority { get; set; }
#else
	public int Priority { get; init; }
#endif
}
