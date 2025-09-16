using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace RedisMemoryCacheInvalidation.Tests.Integration;

[Collection("RedisServer")]
public class InvalidationTests : IClassFixture<RedisServerFixture>
{
    private readonly MemoryCache _localCache;
    private readonly Fixture _fixture;
    private readonly RedisServerFixture _redis;

    private ITestOutputHelper Output { get; }

    public InvalidationTests(RedisServerFixture redisServer, ITestOutputHelper outputHelper)
    {
        Output = outputHelper;

        _localCache = new MemoryCache(Guid.NewGuid().ToString());
        _localCache.Trim(100);

        _redis = redisServer;
        _redis.Reset();

        InvalidationManager.NotificationBus = null;
        InvalidationManager.Configure(_redis.GetEndpoint(), new InvalidationSettings
        {
            InvalidationStrategy = InvalidationStrategyType.All,
            EnableKeySpaceNotifications = true,
            InvalidationCallback = s =>
            {
                Output.WriteLine("RedisCacheInvalidation : Invalidating {0}, Count: {1}", s, _localCache.GetCount());
            },
            TargetCache = _localCache
        });

        _fixture = new Fixture();
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void MultiplesDeps_WhenChangeMonitor_WhenInvalidation_ShouldRemoved()
    {
        var baseCacheKey = _fixture.Create<string>();
        var invalidationKey = _fixture.Create<string>();

        var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
        var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

        CreateCacheItemAndAdd(_localCache, baseCacheKey + "1", monitor1);
        CreateCacheItemAndAdd(_localCache, baseCacheKey + "2", monitor2);

        Assert.Equal(2, _localCache.GetCount());
        Assert.False(monitor1.IsDisposed, "should not be removed before notification");
        Assert.False(monitor2.IsDisposed, "should not be removed before notification");

        var subscriber = _redis.GetSubscriber();
        subscriber.Publish(RedisChannel.Literal(Constants.DefaultInvalidationChannel), Encoding.Default.GetBytes(invalidationKey));

        // hack wait for notification
        Thread.Sleep(50);

        Assert.False(_localCache.Contains(baseCacheKey + "1"), "cache item should be removed");
        Assert.False(_localCache.Contains(baseCacheKey + "2"), "cache item should be removed");
        Assert.True(monitor1.IsDisposed, "should be disposed");
        Assert.True(monitor2.IsDisposed, "should be disposed");
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task MultiplesDeps_WhenImplicitRemoval_WhenInvalidation_ShouldRemoved()
    {
        var baseCacheKey = _fixture.Create<string>();

        CreateCacheItemAndAdd(_localCache, baseCacheKey + "1");
        CreateCacheItemAndAdd(_localCache, baseCacheKey + "2");

        Assert.Equal(2, _localCache.GetCount());

        await InvalidationManager.InvalidateAsync(baseCacheKey + "1");
        await InvalidationManager.InvalidateAsync(baseCacheKey + "2");

        Thread.Sleep(50);

        Assert.Equal(0, _localCache.GetCount());
        Assert.False(_localCache.Contains(baseCacheKey + "1"), "cache item should be removed");
        Assert.False(_localCache.Contains(baseCacheKey + "2"), "cache item should be removed");
    }


    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void MultiplesDeps_WhenChangeMonitor_ShouldBeRemoved()
    {
        var baseCacheKey = _fixture.Create<string>();
        var invalidationKey = _fixture.Create<string>();

        var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
        var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

        CreateCacheItemAndAdd(_localCache, baseCacheKey + "1", monitor1);
        CreateCacheItemAndAdd(_localCache, baseCacheKey + "2", monitor2);

        // Verify initial state
        Assert.Equal(2, _localCache.GetCount());
        Assert.True(_localCache.Contains(baseCacheKey + "1"), "cache item 1 should exist initially");
        Assert.True(_localCache.Contains(baseCacheKey + "2"), "cache item 2 should exist initially");

        // Use direct Redis publish to test change monitor functionality
        var subscriber = _redis.GetSubscriber();
        subscriber.Publish(RedisChannel.Literal(Constants.DefaultInvalidationChannel), invalidationKey);

        // Wait for notification to be processed
        Thread.Sleep(100);

        Assert.Equal(0, _localCache.GetCount());
        Assert.False(_localCache.Contains(baseCacheKey + "1"), "cache item should be removed");
        Assert.False(_localCache.Contains(baseCacheKey + "2"), "cache item should be removed");
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task MultiplesDeps_WhenInvalidationKey_ShouldBeRemoved()
    {
        var baseCacheKey = _fixture.Create<string>();
        var invalidationKey = _fixture.Create<string>();


        var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
        var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

        CreateCacheItemAndAdd(_localCache, baseCacheKey + "1", monitor1);
        CreateCacheItemAndAdd(_localCache, baseCacheKey + "2", monitor2);

        // Verify initial state
        Assert.Equal(2, _localCache.GetCount());
        Assert.True(_localCache.Contains(baseCacheKey + "1"), "cache item 1 should exist initially");
        Assert.True(_localCache.Contains(baseCacheKey + "2"), "cache item 2 should exist initially");

        // Test invalidation by directly invalidating via pub/sub in servicestack redis
        var db = _redis.GetDatabase(0);
        await db.Multiplexer.GetSubscriber().SubscribeAsync(RedisChannel.Literal(Constants.DefaultInvalidationChannel), (channel, message) =>
        {
            Output.WriteLine($"Received message on channel {channel}: {message}");
        });
        await db.PublishAsync(RedisChannel.Literal(Constants.DefaultInvalidationChannel), invalidationKey);

        // Wait for invalidation to be processed
        Thread.Sleep(100);
        Output.WriteLine($"Cache count after invalidation: {_localCache.GetCount()}");

        Assert.Equal(0, _localCache.GetCount());
        Assert.False(_localCache.Contains(baseCacheKey + "1"), "cache item should be removed");
        Assert.False(_localCache.Contains(baseCacheKey + "2"), "cache item should be removed");
    }

    private static CacheItem CreateCacheItemAndAdd(MemoryCache target, string cacheKey, RedisChangeMonitor monitor = null)
    {
        var cacheItem = new CacheItem(cacheKey, DateTime.Now);
        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTime.UtcNow.AddDays(1)
        };
        if(monitor != null)
            policy.ChangeMonitors.Add(monitor);
        target.Add(cacheItem, policy);
        return cacheItem;
    }
}
