namespace CacheManager.Config;

/// <summary>
/// Config of Memory cache
/// </summary>
public class MemoryConfig
{
	/// <summary>
	/// Cache Time
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public TimeSpan CacheTime { get; set; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
#endif
}
