using System.Data;
using System.Diagnostics.CodeAnalysis;
using CacheManager.CacheSource;
using CacheManager.SqlServer.Config;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CacheManager.SqlServer.CacheSource;

/// <summary>
/// Get from SqlServer Db
/// </summary>
public class SqlServerDbCacheSourceWithGet : ICacheSourceWithGet
{
	private readonly DbConfig _config;

	/// <summary>
	/// Create Get from SqlServer Db
	/// </summary>
	/// <param name="config">Db Config</param>
	/// <param name="priority">Priority</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public SqlServerDbCacheSourceWithGet(DbConfig config, int priority)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
	}

	/// <summary>
	/// Get from SqlServer Db
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
	public async Task<T?> GetAsync<T>(string key)
	{
#if NET8_0_OR_GREATER
		await using var connection = new SqlConnection(_config.ConnectionString);
#else
		using var connection = new SqlConnection(_config.ConnectionString);
#endif
		var result = await connection.QuerySingleOrDefaultAsync<T>(
			_config.GetQuery,
			new { Key = key },
			commandType: CommandType.Text,
			commandTimeout: _config.TimeOutOnSecond
		).ConfigureAwait(false);

		return result;
	}

	/// <summary>
	/// Stops
	/// </summary>
	public Task StopAsync() => Task.CompletedTask;

	/// <summary>
	/// Dispose
	/// </summary>
	/// <returns></returns>
	public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
#if NET8_0_OR_GREATER
	public int Priority { get; init; }
#else
	public int Priority { get; set; }
#endif
}
