using System;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using RedisMemoryCacheInvalidation.Monitor;
using RedisMemoryCacheInvalidation.Tests;
using RedisMemoryCacheInvalidation.Tests.Fixtures;
using StackExchange.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Integration.Tests;

[Collection("RedisServer")]
public class InvalidationTests
{
    private readonly MemoryCache _localCache;
    private readonly Fixture _fixture;
    private RedisServerFixture _redis;

    public InvalidationTests(RedisServerFixture redisServer)
    {
        _localCache = new MemoryCache(Guid.NewGuid().ToString());
        _localCache.Trim(100);

        redisServer.Reset();
        _redis = redisServer;

        InvalidationManager.NotificationBus = null;
        InvalidationManager.Configure(_redis.GetEndpoint(), new InvalidationSettings
        {
            InvalidationStrategy = InvalidationStrategyType.All,
            EnableKeySpaceNotifications = true,
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

        //act
        var subscriber = _redis.GetSubscriber();
        subscriber.Publish(RedisChannel.Literal(Constants.DEFAULT_INVALIDATION_CHANNEL), Encoding.Default.GetBytes(invalidationKey));

        // hack wait for notification
        Thread.Sleep(50);

        //assert
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

        // act
        await InvalidationManager.InvalidateAsync(baseCacheKey + "1");
        await InvalidationManager.InvalidateAsync(baseCacheKey + "2");

        Thread.Sleep(50);

        //assert
        Assert.Equal(0, _localCache.GetCount());
        Assert.False(_localCache.Contains(baseCacheKey + "1"), "cache item should be removed");
        Assert.False(_localCache.Contains(baseCacheKey + "2"), "cache item should be removed");
    }


    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public void MultiplesDeps_WhenSpaceNotification_ShouldBeRemoved()
    {
        var baseCacheKey = _fixture.Create<string>();
        var invalidationKey = _fixture.Create<string>();

        var monitor1 = InvalidationManager.CreateChangeMonitor(invalidationKey);
        var monitor2 = InvalidationManager.CreateChangeMonitor(invalidationKey);

        CreateCacheItemAndAdd(_localCache, baseCacheKey + "1", monitor1);
        CreateCacheItemAndAdd(_localCache, baseCacheKey + "2", monitor2);

        // act
        var db = _redis.GetDatabase(0);
        db.StringSet(invalidationKey, "notused");

        Thread.Sleep(200);

        //assert
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
