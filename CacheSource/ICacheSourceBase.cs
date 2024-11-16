using System.ComponentModel.DataAnnotations;

namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider
/// </summary>
/// <typeparam name="T">Item to cache</typeparam>
public interface ICacheSourceBase
{
    /// <summary>
    /// Priority, Lowest priority - checked last
    /// </summary>
    /// <remarks>
    /// The value of <see cref="Priority"/> should be between 1 and 100.
    /// </remarks>
    [Range(1, 100)]
#if NETSTANDARD2_0 || NET462
     int Priority { get; set; }
#else
    int Priority { get; init; }
#endif
}