using System.ComponentModel.DataAnnotations;

namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider
/// </summary>
public interface ICacheSourceBase : IAsyncDisposable
{
	/// <summary>
	/// Stop cache source
	/// </summary>
	/// <returns></returns>
	Task StopAsync();

	/// <summary>
	/// Priority, Lowest priority - checked last
	/// </summary>
	/// <remarks>
	/// The value of <see cref="Priority"/> should be between 1 and 100.
	/// </remarks>
	[Range(1, 100)]
#if NET8_0_OR_GREATER
	int Priority { get; init; }
#else
	int Priority { get; set; }
#endif
}
