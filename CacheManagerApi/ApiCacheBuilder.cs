using CacheManager;
using CacheManagerApi.CacheSource;
using CacheManagerApi.Config;

namespace CacheManagerApi;

/// <summary>
/// Add Api
/// </summary>
public static class ApiCacheBuilder
{
	/// <summary>
	/// Add Api
	/// </summary>
	/// <param name="builder">CacheBuilder</param>
	/// <param name="apiConfig">Config</param>
	/// <param name="priority">Default is 4</param>
	/// <returns>CacheBuilder</returns>
	public static CacheBuilder AddApi(this CacheBuilder builder, ApiConfig apiConfig, int priority = 4)
	{
#if NET8_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(apiConfig);
#else
		if (builder is null)
		{
			throw new ArgumentNullException(nameof(builder));
		}

		if (apiConfig is null)
		{
			throw new ArgumentNullException(nameof(apiConfig));
		}
#endif

		_ = builder.AddCustom(new ApiCacheSourceWithGet(apiConfig, priority));

		return builder;
	}
}
