namespace CacheManager.Config;

/// <summary>
/// Config of Memory cache
/// </summary>
public class MemoryConfig
{
    /// <summary>
    /// Cache Time
    /// </summary>
    public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
}