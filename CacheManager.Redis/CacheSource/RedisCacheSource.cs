using System.Security.Cryptography;
using System.Text.Json;
using CacheManager.CacheSource;
using CacheManager.Redis.Config;
using StackExchange.Redis;

namespace CacheManager.Redis.CacheSource;

/// <summary>
/// Get from Redis
/// </summary>
public class RedisCacheSource : ICacheSourceWithGetWithSetAndClear
{
	private readonly IDatabase _redisCache;
	private readonly RedisConfig _config;
	private readonly string _keyPrefix;

	/// <summary>
	/// Create Get from Api
	/// </summary>
	/// <param name="config">Api Config</param>
	/// <param name="priority">Priority</param>
	/// <param name="keyPrefix">keyPrefix so we can delete all by that, default is "{systemName},{randomNumberBtw1And1000},"</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public RedisCacheSource(RedisConfig config, int priority, string? keyPrefix = null)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
		var connectionMultiplexer = ConnectionMultiplexer.Connect(config.ConnectionString);
		_redisCache = connectionMultiplexer.GetDatabase();

		if (string.IsNullOrEmpty(keyPrefix))
		{
			var systemName = Environment.MachineName;
			var randomNumber = GetCryptographicallySecureRandomNumber(1, 1000);

			_keyPrefix = $"{systemName},{randomNumber},";
		}
		else
		{
			_keyPrefix = keyPrefix!;
		}
	}

	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	public async Task<T?> GetAsync<T>(string key)
	{
		var prefixedKey = $"{_keyPrefix}{key}";

		var value = await _redisCache.StringGetAsync(prefixedKey).ConfigureAwait(false);
		return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
	}

	/// <summary>
	/// Set data to cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="data">Data to cache</param>
	public async Task SetAsync<T>(string key, T data)
	{
		var prefixedKey = $"{_keyPrefix}{key}";

		_ = await _redisCache.StringSetAsync(prefixedKey, JsonSerializer.Serialize(data), _config.CacheTime).ConfigureAwait(false);
	}

	/// <summary>
	/// Clear from cache with key
	/// </summary>
	/// <param name="key"></param>
	public async Task ClearAsync(string key)
	{
		var prefixedKey = $"{_keyPrefix}{key}";

		_ = await _redisCache.KeyDeleteAsync(prefixedKey).ConfigureAwait(false);
	}

	/// <summary>
	/// Clear all from cache
	/// </summary>
	public async Task ClearAllAsync()
	{
		var connectionMultiplexer = _redisCache.Multiplexer;
		var server = connectionMultiplexer.GetServer(_config.ConnectionString);
		var pattern = $"{_keyPrefix}*";

		await foreach (var key in server.KeysAsync(pattern: pattern))
		{
			_ = await _redisCache.KeyDeleteAsync(key).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Generate a cryptographically secure random number
	/// </summary>
	/// <param name="minValue">Minimum value (inclusive)</param>
	/// <param name="maxValue">Maximum value (exclusive)</param>
	/// <returns>Random number</returns>
	private static int GetCryptographicallySecureRandomNumber(int minValue, int maxValue)
	{
		using var rng = RandomNumberGenerator.Create();
		var randomBytes = new byte[4];
		rng.GetBytes(randomBytes);
		var randomValue = BitConverter.ToInt32(randomBytes, 0);

		return (Math.Abs(randomValue) % (maxValue - minValue)) + minValue;
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
