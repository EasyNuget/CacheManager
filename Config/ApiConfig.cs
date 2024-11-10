namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public abstract class ApiConfig
{
    /// <summary>
    /// Url to get Data
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Head
    /// </summary>
    public required Dictionary<string, string>? Header { get; init; } = null;

    /// <summary>
    /// Type
    /// </summary>
    public required ApiType Type { get; init; } = ApiType.GET;

    /// <summary>
    /// TimeOut
    /// </summary>
    public required TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
}