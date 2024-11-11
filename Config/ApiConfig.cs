namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// Url to get Data
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Head
    /// </summary>
    public Dictionary<string, string>? Header { get; init; } = null;

    /// <summary>
    /// Type
    /// </summary>
    public ApiType Type { get; init; } = ApiType.Get;

    /// <summary>
    /// TimeOut
    /// </summary>
    public TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
}