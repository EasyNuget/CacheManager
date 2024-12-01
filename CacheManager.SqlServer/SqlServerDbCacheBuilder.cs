using CacheManager.SqlServer.CacheSource;
using CacheManager.SqlServer.Config;

namespace CacheManager.SqlServer;

/// <summary>
/// Add Database Cache
/// </summary>
public static class SqlServerDbCacheBuilder
{
	/// <summary>
	/// Add Sql Server Database Cache just with Get
	/// </summary>
	/// <param name="builder">CacheBuilder</param>
	/// <param name="dbConfig">Config</param>
	/// <param name="priority">Default is 3</param>
	/// <returns>CacheBuilder</returns>
	public static CacheBuilder AddSqlServerWithGet(this CacheBuilder builder, DbConfig dbConfig, int priority = 3)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(dbConfig);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (dbConfig is null)
		{
			throw new ArgumentNullException(nameof(dbConfig));
		}
#endif

		_ = builder.AddCustom(new SqlServerDbCacheSourceWithGet(dbConfig, priority));

		return builder;
	}

	/// <summary>
	/// Add Sql Server Database Cache with Get and Set and Clear
	/// </summary>
	/// <param name="builder">CacheBuilder</param>
	/// <param name="dbConfig">Config</param>
	/// <param name="priority">Default is 3</param>
	/// <returns>CacheBuilder</returns>
	public static CacheBuilder AddSqlServerWithGetAndSetAndClear(this CacheBuilder builder, DbWithSetAndClearConfig dbConfig, int priority = 3)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(dbConfig);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (dbConfig is null)
		{
			throw new ArgumentNullException(nameof(dbConfig));
		}
#endif

		_ = builder.AddCustom(new SqlServerDbCacheSourceWithGetAndSetAndClear(dbConfig, priority));

		return builder;
	}
}
