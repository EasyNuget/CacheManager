using System.Data;
using System.Text.Json;
using CacheManager.Config;
using Dapper;
using Flurl.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public class EasyCacheManager<T> : IEasyCacheManager<T>
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _redisCache;
    private readonly DbConfig _dbConfig;
    private readonly TimeSpan _cacheTime;
    private readonly ApiConfig _apiCall;

    private readonly bool _isMemoryCacheEnabled;
    private readonly bool _isRedisCacheEnabled;
    private readonly bool _isDbCacheEnabled;

    #region Constructor

    /// <summary>
    /// Get new Manage Cache Easily
    /// </summary>
    private EasyCacheManager(ApiConfig api, TimeSpan cacheTime)
    {
        _isRedisCacheEnabled = true;
        _isMemoryCacheEnabled = true;
        _isDbCacheEnabled = true;

        _apiCall = api;
        _cacheTime = cacheTime;
    }

    /// <summary>
    /// Get new Manage Cache Easily with memory, redis, db
    /// </summary>
    public EasyCacheManager(IMemoryCache memoryCache, RedisConfig redis, DbConfig db, ApiConfig api, TimeSpan cacheTime)
        : this(api, cacheTime)
    {
        _memoryCache = memoryCache;

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redis.ConnectionString);
        _redisCache = connectionMultiplexer.GetDatabase();

        _dbConfig = db;
    }

    /// <summary>
    /// Get new Manage Cache Easily with redis, db
    /// </summary>
    public EasyCacheManager(RedisConfig redis, DbConfig db, ApiConfig api, TimeSpan cacheTime)
        : this(api, cacheTime)
    {
        _isMemoryCacheEnabled = false;

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redis.ConnectionString);
        _redisCache = connectionMultiplexer.GetDatabase();

        _dbConfig = db;
    }

    /// <summary>
    /// Get new Manage Cache Easily with memory, db
    /// </summary>
    public EasyCacheManager(IMemoryCache memoryCache, DbConfig db, ApiConfig api, TimeSpan cacheTime)
        : this(api, cacheTime)
    {
        _isRedisCacheEnabled = false;

        _memoryCache = memoryCache;

        _dbConfig = db;
    }

    /// <summary>
    /// Get new Manage Cache Easily with memory, redis
    /// </summary>
    public EasyCacheManager(IMemoryCache memoryCache, RedisConfig redis, ApiConfig api, TimeSpan cacheTime)
        : this(api, cacheTime)
    {
        _isDbCacheEnabled = false;

        _memoryCache = memoryCache;

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redis.ConnectionString);
        _redisCache = connectionMultiplexer.GetDatabase();
    }

    #endregion

    /// <summary>
    /// Get Cached item from: memory, redis, db. if not exist call api and returned it, also cached it
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>cached item</returns>
    public async Task<T?> GetAsync(string key)
    {
        var specificKey = $"{typeof(T).Name}_{key}";

        // 1. Try to get from memory cache
        if (_isMemoryCacheEnabled && _memoryCache.TryGetValue(specificKey, out T? result))
            return result;

        // 2. Try to get from Redis
        if (_isRedisCacheEnabled)
        {
            var redisValue = await _redisCache.StringGetAsync(specificKey);
            if (redisValue.HasValue)
            {
                result = Deserialize(redisValue);
                _memoryCache.Set(specificKey, result, _cacheTime);
                return result;
            }
        }

        // 3. Try to get from Database
        if (_isDbCacheEnabled)
        {
            result = await GetFromDb();

            SetToMemory(specificKey, result);
            await SetToRedis(specificKey, result);

            return result;
        }

        // 4. Get from API and cache the result
        result = await GetFromApi();
        if (result != null)
        {
            SetToMemory(specificKey, result);
            await SetToRedis(specificKey, result);
        }

        return result;
    }

    /// <summary>
    /// Clear cached item from memory and redis with key
    /// </summary>
    /// <param name="key">key</param>
    public async Task ClearCacheAsync(string key)
    {
        var specificKey = $"{typeof(T).Name}_{key}";

        _memoryCache.Remove(specificKey);
        await _redisCache.KeyDeleteAsync(specificKey);
    }

    #region Private

    private static string Serialize(T? item) => JsonSerializer.Serialize(item);
    private static T? Deserialize(RedisValue value) => JsonSerializer.Deserialize<T>(value!);

    private async Task SetToRedis(string key, T data)
    {
        if (_isRedisCacheEnabled)
        {
            await _redisCache.StringSetAsync(key, Serialize(data), _cacheTime);
        }
    }

    private void SetToMemory(string key, T data)
    {
        if (_isMemoryCacheEnabled)
        {
            _memoryCache.Set(key, data, _cacheTime);
        }
    }

    private async Task<T> GetFromDb()
    {
        await using var connection = new SqlConnection(_dbConfig.ConnectionString);

        var result = await connection.QuerySingleAsync<T>(
            _dbConfig.Query,
            commandType: CommandType.Text,
            commandTimeout: _dbConfig.TimeOutOnSecond);

        return result;
    }

    private async Task<T> GetFromApi()
    {
        T result;

        switch (_apiCall.Type)
        {
            case ApiType.GET:
                result = await _apiCall.Url
                    .WithHeaders(_apiCall.Header)
                    .WithTimeout(_apiCall.TimeOut)
                    .GetJsonAsync<T>();
                break;

            case ApiType.POST:
                result = await _apiCall.Url
                    .WithHeaders(_apiCall.Header)
                    .WithTimeout(_apiCall.TimeOut)
                    .PostJsonAsync(null)
                    .ReceiveJson<T>();
                break;

            default:
                throw new ArgumentException("Not Valid type for api call");
        }


        return result;
    }

    #endregion
}