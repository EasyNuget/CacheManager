namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class RedisConfig
{
    /// <summary>
    /// Connection String for db connect
    /// </summary>
    public required string ConnectionString { get; init; }
    
    /// <summary>
    /// Cache Time
    /// </summary>
    public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
}