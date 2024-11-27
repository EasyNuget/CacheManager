namespace CacheManager.Config;

/// <summary>
/// Config of Memory cache
/// </summary>
public class MemoryConfig
{
	/// <summary>
	/// Cache Time
	/// </summary>
#if NET8_0_OR_GREATER
	public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan CacheTime { get; set; } = TimeSpan.FromSeconds(5);
#endif
}
