using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using AsyncKeyedLock;
using CacheManager.CacheSource;
using CacheManager.Config;

namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public class EasyCacheManager<T> : IEasyCacheManager<T>
{
    private readonly LockConfig _lockConfig;
    private readonly IEnumerable<IBaseCacheSource<T>> _cacheSources;
    private readonly IEnumerable<ICacheSourceWithSet<T>> _cacheSourcesWithSet;
    private readonly IEnumerable<ICacheSourceWithClear<T>> _cacheSourcesWithClear;

    private readonly AsyncKeyedLocker<string> _asyncLock;
    private readonly ConcurrentDictionary<string, Task> _ongoingOperations;

    /// <summary>
    /// Create Manage Cache Easily
    /// </summary>
    /// <param name="cacheSources"></param>
    /// <param name="lockConfig">lock option</param>
    /// <exception cref="ArgumentException">CacheSources is null</exception>
    /// <exception cref="ArgumentException">Options is null</exception>
    /// <exception cref="ArgumentException">Duplicate priority values found</exception>
    public EasyCacheManager([Required] IEnumerable<IBaseCacheSource<T>> cacheSources, LockConfig lockConfig)
    {
        if (cacheSources is null)
            throw new ArgumentException("CacheSources is null", nameof(cacheSources));

        _lockConfig = lockConfig ?? throw new ArgumentException("Options is null", nameof(lockConfig));

        var baseCacheSources = cacheSources.ToList();

        // Check for duplicate priorities
        var duplicatePriorities = baseCacheSources
            .GroupBy(source => source.Priority)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicatePriorities.Count != 0)
            throw new ArgumentException($"Duplicate priority values found: {string.Join(", ", duplicatePriorities)}", nameof(cacheSources));

        _cacheSources = baseCacheSources.OrderBy(x => x.Priority);
        _cacheSourcesWithSet = baseCacheSources.OfType<ICacheSourceWithSet<T>>().OrderBy(x => x.Priority);
        _cacheSourcesWithClear = baseCacheSources.OfType<ICacheSourceWithClear<T>>().OrderBy(x => x.Priority);

        _asyncLock = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions
        {
            PoolSize = Environment.ProcessorCount * lockConfig.PoolSize,
            PoolInitialFill = lockConfig.PoolInitialFill,
            MaxCount = lockConfig.MaxCount
        });

        _ongoingOperations = new ConcurrentDictionary<string, Task>();
    }

    /// <summary>
    /// Get Cached item from all. if not exist call api and returned it, also cached it
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns>cached item</returns>
    public async Task<T?> GetAsync(string key)
    {
        T? result = default;

        foreach (var source in _cacheSources)
        {
            result = await source.GetAsync(key);

            if (result != null)
            {
                if (_ongoingOperations.TryGetValue(key, out var ongoingTask))
                {
                    await ongoingTask;
                }

                using (await _asyncLock.LockOrNullAsync(key, _lockConfig.TimeOut))
                {
                    try
                    {
                        // Double-check if another thread has completed the operation
                        if (_ongoingOperations.TryGetValue(key, out ongoingTask))
                        {
                            await ongoingTask;
                        }

                        var task = SetToListAsync(key, result, source.Priority);
                        _ongoingOperations.TryAdd(key, task);

                        await task;

                        break;
                    }
                    finally
                    {
                        _ongoingOperations.TryRemove(key, out _);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Manual set value to cache
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="value">Value</param>
    /// <returns>cached item</returns>
    public async Task SetAsync(string key, T value)
    {
        using (await _asyncLock.LockAsync(key))
        {
            // Clear all sources in parallel
            var clearTasks = _cacheSourcesWithClear.Select(source => source.ClearAsync(key)).ToList();

            await Task.WhenAll(clearTasks);

            // Set all sources in parallel
            var setTasks = _cacheSourcesWithSet.Select(source => source.SetAsync(key, value)).ToList();

            await Task.WhenAll(setTasks);
        }
    }

    /// <summary>
    /// Clear cached item from all
    /// </summary>
    /// <param name="key">Key</param>
    public async Task ClearCacheAsync(string key)
    {
        using (await _asyncLock.LockAsync(key))
        {
            // Clear all sources in parallel
            var clearTasks = _cacheSourcesWithClear.Select(source => source.ClearAsync(key)).ToList();

            await Task.WhenAll(clearTasks);
        }
    }

    /// <summary>
    /// Dispose of AsyncKeyedLock resources asynchronously
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _asyncLock.Dispose();
        GC.SuppressFinalize(this);

#if NETSTANDARD2_0 || NET462
        return default;
#else
        return ValueTask.CompletedTask;
#endif
    }

    private async Task SetToListAsync(string key, T result, int priority)
    {
        foreach (var higherPrioritySource in _cacheSourcesWithSet.Where(x => x.Priority < priority))
        {
            await higherPrioritySource.SetAsync(key, result);
        }
    }
}