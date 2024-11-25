Manage Cache Easily

### Usage

First install package:

> https://www.nuget.org/packages/EasyMultiCacheManager

then you can use like this:

```csharp
var easyCacheManager = new CacheBuilder()
            .AddApi(new ApiConfig
            {
                Url = StaticData.Api
            })
            .AddRedis(new RedisConfig
            {
                ConnectionString = redisConnectionString
            })
            .AddDb(new DbConfig
            {
                ConnectionString = sqlConnectionString,
                Query = StaticData.QueryToSelect
            })
            .AddMemory(new MemoryConfig())
            .Build(new LockConfig());

var result = await easyCacheManager.GetAsync<string>("My-Key");
```
You can create your own provider with these interfaces:

 - IBaseCacheSource
 - ICacheSourceWithClear
 - ICacheSourceWithSet
 - ICacheSourceWithSetAndClear

Default providers are these:

 - MemoryCacheSource : ICacheSourceWithSetAndClear : Priority => 1
 - RedisCacheSource : ICacheSourceWithSetAndClear : Priority => 2
 - DbCacheSource : IBaseCacheSource : Priority => 3
 - ApiCacheSource : IBaseCacheSource : Priority => 4

On `IBaseCacheSource` you have only `Get` from cache, for example on `ApiCacheSource` you get real data from api.  
if item find from one provider and not exist on other provider, it will automatically set to other providers that implemented from `ICacheSourceWithSetAndClear` or `ICacheSourceWithSet`  

### Example
First try get from Memory because it `Priority` is 1, if not exist it try get from redis, after that it try to get from db, and after that it get data from api. if not found it will return null.  
also if found on db or api it will set to redis and memory.  

### Concurrency
This package use AsyncKeyedLock to handle lock on get, set and clear on specific key. so you use this package on multi thread program.  

### Info
You can use all type like class, object, string to cache, for example EasyCacheManager, EasyCacheManager<MyClass>  
Priority should be unique, you can't crate EasyCacheManager with list of providers with same Priority

### Clear cache
With `ClearCacheAsync` method you can clear specific key on all providers that implement `ICacheSourceWithClear` or `ICacheSourceWithSetAndClear`  

### Set
With `SetAsync` you can manually set value to provides that that implement `ICacheSourceWithSet` or `ICacheSourceWithSetAndClear`


## Contributors

The following contributors have made this project better:

[See the full list of contributors](./CONTRIBUTORS.md)


