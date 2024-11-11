namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class RedisConfig
{
    /// <summary>
    /// Connection String for db connect
    /// </summary>
#if NETSTANDARD2_0 || NET462
    public string ConnectionString { get; set; }
#else
    public required string ConnectionString { get; init; }
#endif


    /// <summary>
    /// Cache Time
    /// </summary>
#if NETSTANDARD2_0 || NET462
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromSeconds(5);
#else
   public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
#endif
}