namespace CacheManager.Config;

/// <summary>
/// Config for lock keys
/// </summary>
public class LockConfig
{
    /// <summary>
    /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
    /// </summary>
    public int MaxCount { get; init; } = 1;

    /// <summary>
    /// The size of the pool to use in order for generated objects to be reused. This is NOT a concurrency limit,
    /// but if the pool is empty then a new object will be created rather than waiting for an object to return to
    /// the pool. Set to 0 to disable pooling (strongly recommended to use). Defaults to 20.
    /// used like this on code: Environment.ProcessorCount * PoolSize
    /// </summary>
    public int PoolSize { get; init; } = 20;

    /// <summary>
    /// The number of items to fill the pool with during initialization. A value of -1 means to fill up to the pool size. Defaults to 1.
    /// </summary>
    public int PoolInitialFill { get; init; } = 1;

    /// <summary>
    /// Time Out
    /// </summary>
    public TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
}