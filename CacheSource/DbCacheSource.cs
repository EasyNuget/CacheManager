using System.Data;
using CacheManager.Config;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Db
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class DbCacheSource<T> : IBaseCacheSource<T>
{
    private readonly DbConfig _config;

    /// <summary>
    /// Create Get from Api
    /// </summary>
    /// <param name="config">Api Config</param>
    /// <exception cref="ArgumentException">Config is null</exception>
    public DbCacheSource(DbConfig config)
    {
        _config = config ?? throw new ArgumentException("Config is null", nameof(config));
    }

    /// <summary>
    /// Get from cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>Result</returns>
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
            commandTimeout: _config.TimeOutOnSecond);

        return result;
    }

    /// <summary>
    /// Priority, Lowest priority - checked last
    /// </summary>
    public int Priority => 3;
}