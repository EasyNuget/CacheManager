namespace CacheManager.Config;

/// <summary>
/// Config for lock keys
/// </summary>
public class LockConfig
{
	/// <summary>
	/// The size of the pool to use in order for generated objects to be reused. This is NOT a concurrency limit,
	/// but if the pool is empty then a new object will be created rather than waiting for an object to return to
	/// the pool. Set to 0 to disable pooling (strongly recommended to use). Defaults to 20.
	/// used like this on code: Environment.ProcessorCount * PoolSize
	/// </summary>
#if NET8_0_OR_GREATER
	public int PoolSize { get; init; } = 20;
#else
	public int PoolSize { get; set; } = 20;
#endif

	/// <summary>
	/// The number of items to fill the pool with during initialization. A value of -1 means to fill up to the pool size. Defaults to 1.
	/// </summary>
#if NET8_0_OR_GREATER
	public int PoolInitialFill { get; init; } = 1;
#else
	public int PoolInitialFill { get; set; } = 1;
#endif

	/// <summary>
	/// Time Out
	/// </summary>
#if NET8_0_OR_GREATER
	public TimeSpan TimeOut { get; init; } = TimeSpan.FromSeconds(5);
#else
	public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(5);
#endif
}
