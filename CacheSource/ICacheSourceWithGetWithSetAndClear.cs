namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with clear and set ability
/// </summary>
public interface ICacheSourceWithGetWithSetAndClear : ICacheSourceWithGetWithSet, ICacheSourceWithGetWithClear
{
}
