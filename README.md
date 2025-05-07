Manage Cache Easily

### Usage

First install package:

> https://www.nuget.org/packages/EasyMultiCacheManager

If you want Redis then install:  

> https://www.nuget.org/packages/EasyMultiCacheManager.Redis

If you want Database then install:

> https://www.nuget.org/packages/EasyMultiCacheManager.Database

If you want Api then install:

> https://www.nuget.org/packages/EasyMultiCacheManager.Api  

Then you can use like this:  

```csharp
var easyCacheManager = new CacheBuilder()
            .AddApi(new ApiConfig
            {
                Url = StaticData.Api
            })
            .AddSqlServerWithGet(new DbConfig
            {
                ConnectionString = sqlConnectionString,
                GetQuery = StaticData.GetQuery
            })
            .AddRedis(new RedisConfig
            {
                ConnectionString = redisConnectionString
            })
            .AddMemory(new MemoryConfig())
            .Build(new LockConfig());

var result = await easyCacheManager.GetAsync<string>("My-Key");
```

```csharp
var easyCacheManager = new CacheBuilder()
            .AddApi(new ApiConfig
            {
                Url = StaticData.Api
            })
            .AddSqlServerWithGetAndSetAndClear(new DbWithSetAndClearConfig
            {
                ConnectionString = sqlConnectionString,
                GetQuery = StaticData.GetQuery,
                SetQuery = StaticData.SetQuery,
                ClearQuery = StaticData.ClearQuery,
                ClearAllQuery = StaticData.ClearAllQuery,
            })
            .AddRedis(new RedisConfig
            {
                ConnectionString = redisConnectionString
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
With `ClearAllCacheAsync` method you can clear all keys on all providers that implement `ICacheSourceWithClear` or `ICacheSourceWithSetAndClear`

### Set
With `SetAsync` you can manually set value to provides that that implement `ICacheSourceWithSet` or `ICacheSourceWithSetAndClear`

### Clear Event
If you want to have ability to clear cache on all solutions you can use these packages.  

If you want Redis event install:

> https://www.nuget.org/packages/EasyMultiCacheManager.Clear.Redis

If you want Kafka event then install:

> https://www.nuget.org/packages/EasyMultiCacheManager.Clear.Kafka

If you want rabbitMQ event then install:

> https://www.nuget.org/packages/EasyMultiCacheManager.Clear.Rabbit

Now you can use like this:

```csharp
using CacheManager;
using CacheManagerClear;
using CacheManagerClear.Kafka;
using Confluent.Kafka;

class Program
{
    static async Task Main(string[] args)
    {
        // Kafka configuration
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "cache-clear-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var topic = "cache_clear_topic";

        // Cache manager
        IEasyCacheManager cacheManager = new EasyCacheManager();

        // Initialize producer and consumer
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        using var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();

        // Create CacheManagerClearBuilder
        var builder = new CacheManagerClearBuilder();

        // Configure Kafka Publisher
        builder.UseKafkaPublisher(producer, topic);

        // Configure Kafka Subscriber
        builder.UseKafkaSubscriber(consumer, topic, cacheManager);

        // Alternatively, configure both in one call
        // builder.UseKafkaPublisherAndSubscriber(producer, consumer, topic, cacheManager);

        // Build and get publisher and subscriber
        var (publisher, subscriber) = builder.BuildPublisherAndSubscriber(
            new CachePublisher(producer, topic),
            new CacheSubscriber(consumer, topic, cacheManager)
        );

        // Subscribe to cache clear events
        await subscriber.SubscribeAsync();

        // Publish a cache clear event
        await publisher.PublishClearCacheAsync("sample_cache_key");

        Console.WriteLine("Cache clear event published and subscriber listening...");
        Console.ReadLine();
    }
}
```

## Contributors
The following contributors have made this project better:

[See the full list of contributors](./CONTRIBUTORS.md)

## Wiki
Document generated by deepwiki:  
[wiki](https://deepwiki.com/EasyNuget/CacheManager)


