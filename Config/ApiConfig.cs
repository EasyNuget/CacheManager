namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public class ApiConfig
{
	/// <summary>
	/// Url to get Data
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public string Url { get; set; }
#else
	public required string Url { get; init; }
#endif

	/// <summary>
	/// Head
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public Dictionary<string, string>? Header { get; set; } = null;
#else
	public Dictionary<string, string>? Header { get; init; }
#endif

	/// <summary>
	/// Type
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public ApiType Type { get; set; } = ApiType.Get;
#else
	public ApiType Type { get; init; } = ApiType.Get;
#endif

	/// <summary>
	/// TimeOut
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
#endif
}
