using CacheManager;
using CacheManager.CacheSource;
using CacheManagerApi.Config;
using Flurl.Http;

namespace CacheManagerApi.CacheSource;

/// <summary>
/// Get from Api
/// </summary>
public class ApiCacheSourceWithGet : ICacheSourceWithGet
{
	private readonly ApiConfig _config;

	/// <summary>
	/// Create Get from Api
	/// </summary>
	/// <param name="config">Api Config</param>
	/// <param name="priority">Priority</param>
	/// <exception cref="ArgumentException">Config is null</exception>
	public ApiCacheSourceWithGet(ApiConfig config, int priority)
	{
		Priority = priority;
		_config = config ?? throw new ArgumentException(Resources.NullValue, nameof(config));
	}

	/// <summary>
	/// Get from cache
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>Result</returns>
	public async Task<T?> GetAsync<T>(string key)
	{
		var result = _config.Type switch
		{
			ApiType.Get => await _config.Url
				.WithHeaders(_config.Header)
				.WithTimeout(_config.TimeOut)
				.GetJsonAsync<T?>().ConfigureAwait(false),

			ApiType.Post => await _config.Url
				.WithHeaders(_config.Header)
				.WithTimeout(_config.TimeOut)
				.PostJsonAsync(null)
				.ReceiveJson<T?>().ConfigureAwait(false),

			_ => throw new ArgumentException(Resources.NullValue)
		};

		return result;
	}

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
#if NET8_0_OR_GREATER
	public int Priority { get; init; }
#else
	public int Priority { get; set; }
#endif
}
