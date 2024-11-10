namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public abstract class RedisConfig
{
    /// <summary>
    /// Connection String for db connect
    /// </summary>
    public required string ConnectionString { get; init; }
}