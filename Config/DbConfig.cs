namespace CacheManager.Config;

/// <summary>
/// Config of Database cache
/// </summary>
public class DbConfig
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
    /// Query that get data from db
    /// </summary>
#if NETSTANDARD2_0 || NET462
    public string Query { get; set; }
#else
    public required string Query { get; init; }
#endif

    /// <summary>
    /// TimeOut
    /// </summary>
#if NETSTANDARD2_0 || NET462
    public int TimeOutOnSecond { get; set; } = 5;
#else
    public int TimeOutOnSecond { get; init; } = 5;
#endif
}