namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class ApiConfig
{
	/// <summary>
	/// Url to get Data
	/// </summary>
#if NET8_0_OR_GREATER
	public required string Url { get; init; }
#else
	public string Url { get; set; }
#endif

	/// <summary>
	/// Head
	/// </summary>
#if NET8_0_OR_GREATER
	public Dictionary<string, string>? Header { get; init; }
#else
	public Dictionary<string, string>? Header { get; set; } = null;
#endif

	/// <summary>
	/// Type
	/// </summary>
#if NET8_0_OR_GREATER
	public ApiType Type { get; init; } = ApiType.Get;
#else
	public ApiType Type { get; set; } = ApiType.Get;
#endif

	/// <summary>
	/// TimeOut
	/// </summary>
#if NET8_0_OR_GREATER
	public TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(5);
#endif
}
