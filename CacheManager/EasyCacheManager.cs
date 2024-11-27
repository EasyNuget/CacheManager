using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AsyncKeyedLock;
using CacheManager.CacheSource;
using CacheManager.Config;

namespace CacheManager;

/// <summary>
/// Manage Cache Easily
/// </summary>
public class EasyCacheManager : IEasyCacheManager
{
	private readonly LockConfig _lockConfig;
	private readonly IEnumerable<ICacheSourceWithGet> _cacheSources;
	private readonly IEnumerable<ICacheSourceWithGetWithSet> _cacheSourcesWithSet;
	private readonly IEnumerable<ICacheSourceWithGetWithClear> _cacheSourcesWithClear;

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
	public EasyCacheManager([Required] IEnumerable<ICacheSourceWithGet> cacheSources, LockConfig lockConfig)
	{
		if (cacheSources is null)
		{
			throw new ArgumentException(Resources.NullValue, nameof(cacheSources));
		}

		_lockConfig = lockConfig ?? throw new ArgumentException(Resources.NullValue, nameof(lockConfig));

		var baseCacheSources = cacheSources.ToList();

		// Check for duplicate priorities
		var duplicatePriorities = baseCacheSources
			.GroupBy(source => source.Priority)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicatePriorities.Count != 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.Duplicate_priority, string.Join(", ", duplicatePriorities)), nameof(cacheSources));
		}

		_cacheSources = baseCacheSources.OrderBy(x => x.Priority);
		_cacheSourcesWithSet = baseCacheSources.OfType<ICacheSourceWithGetWithSet>().OrderBy(x => x.Priority);
		_cacheSourcesWithClear = baseCacheSources.OfType<ICacheSourceWithGetWithClear>().OrderBy(x => x.Priority);

		_asyncLock = new AsyncKeyedLocker<string>(new AsyncKeyedLockOptions
		{
			PoolSize = Environment.ProcessorCount * lockConfig.PoolSize, PoolInitialFill = lockConfig.PoolInitialFill, MaxCount = lockConfig.MaxCount
		});

		_ongoingOperations = new ConcurrentDictionary<string, Task>();
	}

	/// <summary>
	/// Get Cached item from all. if not exist call api and returned it, also cached it
	/// </summary>
	/// <param name="key">Key</param>
	/// <returns>cached item</returns>
	public async Task<T?> GetAsync<T>(string key)
	{
		T? result = default;

		foreach (var source in _cacheSources)
		{
			result = await source.GetAsync<T>(key).ConfigureAwait(false);

			if (result != null)
			{
				if (_ongoingOperations.TryGetValue(key, out var ongoingTask))
				{
					await ongoingTask.ConfigureAwait(false);
				}

				using (await _asyncLock.LockOrNullAsync(key, _lockConfig.TimeOut).ConfigureAwait(false))
				{
					try
					{
						// Double-check if another thread has completed the operation
						if (_ongoingOperations.TryGetValue(key, out ongoingTask))
						{
							await ongoingTask.ConfigureAwait(false);
						}

						var task = SetToListAsync(key, result, source.Priority);
						_ = _ongoingOperations.TryAdd(key, task);

						await task.ConfigureAwait(false);

						break;
					}
					finally
					{
						_ = _ongoingOperations.TryRemove(key, out _);
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
	public async Task SetAsync<T>(string key, T value)
	{
		using (await _asyncLock.LockAsync(key).ConfigureAwait(false))
		{
			// Set all sources in parallel
			var setTasks = _cacheSourcesWithSet.Select(source => source.SetAsync(key, value)).ToList();

			await Task.WhenAll(setTasks).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Clear cached item by key
	/// </summary>
	/// <param name="key">Key</param>
	public async Task ClearCacheAsync(string key)
	{
		using (await _asyncLock.LockAsync(key).ConfigureAwait(false))
		{
			// Clear all sources in parallel
			var clearTasks = _cacheSourcesWithClear.Select(source => source.ClearAsync(key)).ToList();

			await Task.WhenAll(clearTasks).ConfigureAwait(false);
		}
	}

	/// <summary>
	/// Clear cached item from all
	/// </summary>
	public async Task ClearAllCacheAsync()
	{
		// Clear all sources in parallel
		var clearTasks = _cacheSourcesWithClear.Select(source => source.ClearAllAsync()).ToList();

		await Task.WhenAll(clearTasks).ConfigureAwait(false);
	}

	/// <summary>
	/// Dispose of AsyncKeyedLock resources asynchronously
	/// </summary>
	public ValueTask DisposeAsync()
	{
		_asyncLock.Dispose();
		GC.SuppressFinalize(this);

#if NET8_0_OR_GREATER
		return ValueTask.CompletedTask;
#else
		return default;
#endif
	}

	private async Task SetToListAsync<T>(string key, T result, int priority)
	{
		foreach (var higherPrioritySource in _cacheSourcesWithSet.Where(x => x.Priority < priority))
		{
			await higherPrioritySource.SetAsync(key, result).ConfigureAwait(false);
		}
	}
}
