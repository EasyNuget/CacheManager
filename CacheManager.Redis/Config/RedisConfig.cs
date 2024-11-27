namespace CacheManager.Redis.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class RedisConfig
{
	/// <summary>
	/// Connection String for db connect
	/// </summary>
#if NET8_0_OR_GREATER
	public required string ConnectionString { get; init; }
#else
	public string ConnectionString { get; set; }
#endif


	/// <summary>
	/// Cache Time
	/// </summary>
#if NET8_0_OR_GREATER
	public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan CacheTime { get; set; } = TimeSpan.FromSeconds(5);
#endif
}
