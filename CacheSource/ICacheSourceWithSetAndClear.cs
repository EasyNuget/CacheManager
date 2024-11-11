namespace CacheManager.CacheSource;

/// <summary>
/// Base interface of cache provider with clear and set ability
/// </summary>
/// <typeparam name="T">Item to cache</typeparam>
public interface ICacheSourceWithSetAndClear<T> : ICacheSourceWithSet<T>, ICacheSourceWithClear<T>
{
}