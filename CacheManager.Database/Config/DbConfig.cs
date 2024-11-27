namespace CacheManager.Database.Config;

/// <summary>
/// Config of Database cache
/// </summary>
public class DbConfig
{
	/// <summary>
	/// Connection String for db connect
	/// </summary>
#if NET8_0_OR_GREATER
	public required string ConnectionString { get; init; }
#else
	public string ConnectionString { get; set; }

#endif

	/// <summary>
	/// Query that get data from db
	/// </summary>
#if NET8_0_OR_GREATER
	public required string Query { get; init; }
#else
	public string Query { get; set; }
#endif

	/// <summary>
	/// TimeOut
	/// </summary>
#if NET8_0_OR_GREATER
	public int TimeOutOnSecond { get; init; } = 5;
#else
	public int TimeOutOnSecond { get; set; } = 5;
#endif
}
