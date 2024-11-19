using System.Data;
using System.Diagnostics.CodeAnalysis;
using CacheManager.Config;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Db
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class DbCacheSourceWithGet<T> : ICacheSourceWithGet<T>
{
	private readonly DbConfig _config;

	/// <summary>
	/// Create Get from Api
	/// </summary>
	/// <param name="config">Api Config</param>
	/// <param name="priority">Priority</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public DbCacheSourceWithGet(DbConfig config, int priority)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
	}

	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
	public async Task<T?> GetAsync(string key)
	{
#if NETSTANDARD2_0 || NET462
        using var connection = new SqlConnection(_config.ConnectionString);
#else
		await using var connection = new SqlConnection(_config.ConnectionString);
#endif
		var result = await connection.QuerySingleOrDefaultAsync<T>(
			_config.Query,
			new { Key = key },
			commandType: CommandType.Text,
			commandTimeout: _config.TimeOutOnSecond).ConfigureAwait(false);

		return result;
	}

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
#if NETSTANDARD2_0 || NET462
    public int Priority { get; set; }
#else
	public int Priority { get; init; }
#endif
}
