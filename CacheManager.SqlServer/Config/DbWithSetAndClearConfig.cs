namespace CacheManager.SqlServer.Config;

/// <summary>
/// Config of Database cache
/// </summary>
public class DbWithSetAndClearConfig : DbConfig
{
	/// <summary>
	/// Query that set data to db
	/// </summary>
#if NET8_0_OR_GREATER
	public required string SetQuery { get; init; }
#else
	public string SetQuery { get; set; }
#endif

	/// <summary>
	/// Query that clear a data on db
	/// </summary>
#if NET8_0_OR_GREATER
	public required string ClearQuery { get; init; }
#else
	public string ClearQuery { get; set; }
#endif

	/// <summary>
	/// Query that clear all data on db
	/// </summary>
#if NET8_0_OR_GREATER
	public required string ClearAllQuery { get; init; }
#else
	public string ClearAllQuery { get; set; }
#endif
}
