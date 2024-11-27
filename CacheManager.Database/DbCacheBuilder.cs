using CacheManager.Database.CacheSource;
using CacheManager.Database.Config;

namespace CacheManager.Database;

/// <summary>
/// Add Database Cache
/// </summary>
public static class DbCacheBuilder
{
	/// <summary>
	/// Add Database Cache
	/// </summary>
	/// <param name="builder">CacheBuilder</param>
	/// <param name="dbConfig">Config</param>
	/// <param name="priority">Default is 3</param>
	/// <returns>CacheBuilder</returns>
	public static CacheBuilder AddDb(this CacheBuilder builder, DbConfig dbConfig, int priority = 3)
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

		_ = builder.AddCustom(new DbCacheSourceWithGet(dbConfig, priority));

		return builder;
	}
}
