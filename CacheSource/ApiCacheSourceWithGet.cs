using CacheManager.Config;
using Flurl.Http;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Api
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class ApiCacheSourceWithGet<T> : ICacheSourceWithGet<T>
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
	public async Task<T?> GetAsync(string key)
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
#if NETSTANDARD2_0 || NET462
    public int Priority { get; set; }
#else
	public int Priority { get; init; }
#endif
}
