using CacheManager.Config;
using Flurl.Http;

namespace CacheManager.CacheSource;

/// <summary>
/// Get from Api
/// </summary>
/// <typeparam name="T">Result</typeparam>
public class ApiCacheSource<T> : IBaseCacheSource<T>
{
    private readonly ApiConfig _config;

    /// <summary>
    /// Create Get from Api
    /// </summary>
    /// <param name="config">Api Config</param>
    /// <param name="priority">Priority</param>
    /// <exception cref="ArgumentException">Config is null</exception>
    public ApiCacheSource(ApiConfig config, int priority)
    {
        Priority = priority;
        _config = config ?? throw new ArgumentException("Config is null", nameof(config));
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
                .GetJsonAsync<T?>(),

            ApiType.Post => await _config.Url
                .WithHeaders(_config.Header)
                .WithTimeout(_config.TimeOut)
                .PostJsonAsync(null)
                .ReceiveJson<T?>(),

            _ => throw new ArgumentException("Not Valid type for api call")
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