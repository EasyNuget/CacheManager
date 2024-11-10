namespace CacheManager.Config;

/// <summary>
/// Config of Memory cache
/// </summary>
public abstract class MemoryConfig
{
    /// <summary>
    /// Cache Time
    /// </summary>
    public required TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
}