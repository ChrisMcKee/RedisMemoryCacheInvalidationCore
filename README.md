RedisMemoryCacheInvalidation - Core
============================

A migration and changes to RedisMemoryCacheInvalidation as PRs were going stale and the package appears abandoned.

[![Nuget](https://img.shields.io/nuget/dt/RedisMemoryCacheInvalidationCore.svg)](http://nuget.org/packages/RedisMemoryCacheInvalidationCore)
[![Nuget](https://img.shields.io/nuget/v/RedisMemoryCacheInvalidationCore.svg)](http://nuget.org/packages/RedisMemoryCacheInvalidationCore)

[System.Runtime.MemoryCache](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache) invalidation using Redis PubSub feature.

Installing via NuGet
---

```
Install-Package RedisMemoryCacheInvalidationCore
```

How to use it ?
---

__quick start__

First, you have to configure the library, mainly to set up a persistent redis connection and various stuff

```csharp
  // somewhere in your global.asax/startup.cs
  InvalidationManager.Configure("localhost:6379", new InvalidationSettings());
```

Redis connection string
follow [StackExchange.Redis Configuration model](https://github.com/StackExchange/StackExchange.Redis/blob/main/docs/Configuration.md)

There are at least 3 ways to send invalidation messages :

- send an invalidation message via any redis client following the command `PUBLISH invalidate onemessagekey`
- use `InvalidationManager.InvalidateAsync` (same as the previous one)
- use keyspace notification (yes, RedisMemoryCacheInvalidation supports it)

Once an invalidation message is intercepted by the library, you can invalidate one or more items at the same time by
using `InvalidationSettings.InvalidationStrategy`

- `InvalidationStrategyType.ChangeMonitor` => a custom change monitor `InvalidationManager.CreateChangeMonitor`
- `InvalidationStrategyType.AutoCacheRemoval` => use the automatic MemoryCache removal configured at
  `InvalidationSettings.ConfigureAsync`
- `InvalidationStrategyType.External` => use the callback configured at `InvalidationSettings.InvalidationCallback`

How it works ?
---

A single connection is opened to redis via the servicestack thread-safe multiplexer.

The library uses a topic-based observer pattern to manage notifications within the application; allowing for a large
number of possible monitors.

Invalidation messages are sent in the format `channel=invalidate` and `message=key-to-invalidate`.

As the name suggests, this implementation relies on [Redis](http://redis.io/), especially on [PubSub feature](http://redis.io/topics/pubsub).
In the current version of this implementation, __nothing is stored__ on the Redis server.

```text
                       +------------------------------------------+
                       |  System.Runtime.Caching.MemoryCache      |
                       |  clients (Frontend/Backend Servers)      |
                       |------------------------------------------|
                       |   [Server1]   [Server2]   [Server3]      |
                       +------------------------------------------+
                             ^    ^    ^              |
                             |    |    |              |
                             |    |    |              v
                             |    |    |   5 - Remove items from local cache
                             |    |    +---------------------------+
                             |    |                                |
                             |    +--------------------------------+
                             |                                     |
                             |                                     |
                             | 2 - OnDemand: Load data             |
                             |     from Data Tier                  |
                             |                                     |
                       +-------------------+                       |
                       |     Data Tier     |<----------------------+
                       +-------------------+

                             ^
                             | 4 - Send notification
                             |
                       +-------------------+
                       |      Redis        |
                       |  (Invalidations)  |
                       +-------------------+
                          ^             |
        1 - Subscribe     |             | 3 - Publish Invalidation
        on startup        |             |   (when something changed)
                          |             v
                          |       +-------------------+
                          |       |    Publisher      |
                          |       +-------------------+
                          |
                    3' - Publish Invalidation
                        from any redis client
 
```

### Configuration

Settings
---
To configure `RedisMemoryCacheInvalidation`, you should use one of the `InvalidationManager.ConfigureAsync` methods.
Three parameters are available to configure it :

- __redisConfig:string__ : Redis connection string. Check [StackExchange.Redis Configuration model](https://github.com/StackExchange/StackExchange.Redis/blob/master/Docs/Configuration.md) for more details. A basic example is `localhost:6379`.
- __mux:ConnectionMultiplexer__ : an existing StackExchange.Redis.ConnectionMultiplexer that you want to reuse.
- __settings:InvalidationSettings__ : see below for more details.

InvalidationSettings is the main configuration object
- __InvalidationStrategy:InvalidationStrategyType__ : How to handle invalidation notifications : notify ChangeMonitor, execute callback or automatically remove an item from the cache.
- __TargetCache:MemoryCache__ : the target MemoryCache instance when `InvalidationStrategy` is set to `AutoCacheRemoval`.
- __EnableKeySpaceNotifications:bool__ : allow subscribe to keyevents notification `__keyevent*__:`.
- __InvalidationCallback:Action__ : a callback that is invoked when  `InvalidationStrategy` is set to `External`.

When to configure ?
---
Thanks to StackExchange.Redis a persistent connection is established between your application and the redis server.
That's why it's important to configure it very early at startup : Global.asax, Owin or Application Initialisation.

### Examples


Once RedisMemoryCacheInvalidation is configured, local cache invalidation is a two-steps process : capturing invalidation messages and handling those notification messages.


Sending invalidation messages
---
You can use one of the folowing methods.

- Send a pubsub message from any redis client `PUBLISH invalidate onemessagekey`.
- Send an invalidation message from `InvalidationManager.InvalidateAsync("onemessagekey")`
- Capture keyspace events for one particular key. Note : the redis server should be configured to support keyspace events. (off by default)

Handling notification messages
---
This behavior is entirely configured via `InvalidationSettings.InvalidationStrategyType`. As it's marked with a FlagsAttribute, you can use one or more strategies.

- Automatically removed a cache item from the cache

The easiest way to invalidate local cache items. If the  The core will try to remove cache items
For example, if you add a cache item like this :

```
CacheItem cacheItem = new CacheItem("mycacheKey", "cachevalue");
CacheItemPolicy policy = new CacheItemPolicy();
policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
MyCache.Add(cacheItem, policy);
```

Calling  `PUBLISH invalidate mycacheKey` or `InvalidationManager.InvalidateAsync("mycacheKey")` will remove that item from the cache.

- Notify ChangeMonitors

[ChangeMonitor](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.caching.changemonitor) is defined as "Provides a base class for a derived custom type that monitors changes in the state of the data which a cache item depends on.""

You can create a custom monitor (watching for `myinvalidationKey`) like this :

```
CacheItem cacheItem = new CacheItem("cacheKey", "cachevalue");
CacheItemPolicy policy = new CacheItemPolicy();
policy.AbsoluteExpiration = DateTime.UtcNow.AddDays(1);
policy.ChangeMonitors.Add(InvalidationManager.CreateChangeMonitor("myinvalidationKey"));
MyCache.Add(cacheItem, policy);
```

When raised the corresponding cache item will be automatically removed.
One interesting feature is that you can create several change monitors watching for the same key.

- invoke a callback

Suppose you're using another caching implementation there is another way to be notified with `InvalidationStrategyType.External`.
Each time a notification message is intercepted, the callback defined in `InvalidationSettings.InvalidationCallback` is invoked.
It's up to you to remove/flush/reload the cache item.

- Enabling resilience (retrys)

```csharp
var settings = new InvalidationSettings
{
    EnableResilience = true,
    HealthCheckIntervalMs = 15000, // 15 seconds
    MaxRetryAttempts = 5,
    RetryDelayMs = 500
};

InvalidationManager.Configure("localhost:6379", settings);
```

License
---
Licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT)


