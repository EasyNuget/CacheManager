using CacheManager.Redis.CacheSource;
using CacheManager.Redis.Config;

namespace CacheManager.Redis;

/// <summary>
/// Add Redis Cache
/// </summary>
public static class RedisCacheBuilder
{
	/// <summary>
	/// Add Redis Cache
	/// </summary>
	/// <param name="builder">CacheBuilder</param>
	/// <param name="redisConfig">Config</param>
	/// <param name="priority">Default is 2</param>
	/// <returns>CacheBuilder</returns>
	public static CacheBuilder AddRedis(this CacheBuilder builder, RedisConfig redisConfig, int priority = 2)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(redisConfig);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (redisConfig is null)
		{
			throw new ArgumentNullException(nameof(redisConfig));
		}
#endif

		_ = builder.AddCustom(new RedisCacheSource(redisConfig, priority));

		return builder;
	}
}
