namespace CacheManager.SqlServer.Config;

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
	public required string GetQuery { get; init; }
#else
	public string GetQuery { get; set; }
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
