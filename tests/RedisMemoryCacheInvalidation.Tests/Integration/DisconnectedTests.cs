using System;
using System.Threading.Tasks;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Integration;

public class DisconnectedTests
{
    public DisconnectedTests()
    {
        InvalidationManager.NotificationBus = null;
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task InvalidHost_ShouldNotBeConnected()
    {
        //test more disconnected scenarios
        InvalidationManager.Configure("blabblou", new InvalidationSettings());
        Assert.False(InvalidationManager.IsConnected);
        
        // When not connected, InvalidateAsync should return 0 (no subscribers) instead of throwing
        var result = await InvalidationManager.InvalidateAsync("mykey");
        Assert.Equal(0L, result);
    }

    [Fact]
    [Trait(TestConstants.TestCategory, TestConstants.IntegrationTestCategory)]
    public async Task WhenNotConnected_ShouldNotPublishMessages()
    {
        //test more disconnected scenarios
        InvalidationManager.Configure("blabblou", new InvalidationSettings());

        var published = await InvalidationManager.InvalidateAsync("mykey");
        Assert.Equal(0L, published);
    }
}